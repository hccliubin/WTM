using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkUserVms;
using WalkingTec.Mvvm.Core.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WalkingTec.Mvvm.Mvc.Admin.Controllers
{
    [Area("_Admin")]
    [ActionDescription("MenuKey.UserManagement")]
    public class FrameworkUserController : BaseController
    {
        #region 搜索
        [ActionDescription("Sys.Search")]
        public ActionResult Index()
        {
            var vm = Wtm.CreateVM<FrameworkUserListVM>();
            vm.Searcher.IsValid = true;
            return PartialView(vm);
        }

        [ActionDescription("Sys.Search")]
        [HttpPost]
        public string Search(FrameworkUserSearcher searcher)
        {
            var vm = Wtm.CreateVM<FrameworkUserListVM>(passInit: true);
            if (ModelState.IsValid)
            {
                vm.Searcher = searcher;
                return vm.GetJson(false);
            }
            else
            {
                return vm.GetError();
            }
        }

        #endregion

        #region 新建
        [ActionDescription("Sys.Create")]
        public ActionResult Create()
        {
            var vm = Wtm.CreateVM<FrameworkUserVM>();
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.Create")]
        public async Task<ActionResult> Create(FrameworkUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }
            else
            {
                await vm.DoAddAsync();
                if (!ModelState.IsValid)
                {
                    vm.DoReInit();
                    return PartialView(vm);
                }
                else
                {
                    return FFResult().CloseDialog().RefreshGrid();
                }
            }
        }
        #endregion

        #region 修改
        [ActionDescription("Sys.Edit")]
        public ActionResult Edit(string id)
        {
            var vm = Wtm.CreateVM<FrameworkUserVM>(id);
            vm.Entity.Password = null;
            return PartialView(vm);
        }

        [ActionDescription("Sys.Edit")]
        [HttpPost]
        [ValidateFormItemOnly]
        public async Task<ActionResult> Edit(FrameworkUserVM vm)
        {
            if (ModelState.Any(x => x.Key != "Entity.Password" && x.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid))
            {
                return PartialView(vm);
            }
            else
            {
                ModelState.Clear();
                await vm.DoEditAsync();
                if (!ModelState.IsValid)
                {
                    vm.DoReInit();
                    return PartialView(vm);
                }
                else
                {
                    return FFResult().CloseDialog().RefreshGridRow(vm.Entity.ID);
                }
            }
        }
        #endregion

        #region 修改密码
        [ActionDescription("Login.ChangePassword")]
        public ActionResult Password(Guid id)
        {
            var vm = Wtm.CreateVM<FrameworkUserVM>(id,passInit:true);
            vm.Entity.Password = null;
            return PartialView(vm);
        }

        [ActionDescription("Login.ChangePassword")]
        [HttpPost]
        public ActionResult Password(FrameworkUserVM vm)
        {
            var keys = ModelState.Keys.ToList();
            foreach (var item in keys)
            {
                if(item != "Entity.Password")
                {
                    ModelState.Remove(item);
                }
            }
            if (ModelState.IsValid == false)
            {
                return PartialView(vm);
            }
            else
            {
                vm.ChangePassword();
                if (!ModelState.IsValid)
                {
                    vm.DoReInit();
                    return PartialView(vm);
                }
                else
                {
                    return FFResult().CloseDialog().RefreshGridRow(vm.Entity.ID);
                }
            }
        }
        #endregion


        #region 删除
        [ActionDescription("Sys.Delete")]
        public ActionResult Delete(Guid id)
        {
            var vm = Wtm.CreateVM<FrameworkUserVM>(id);
            return PartialView(vm);
        }

        [ActionDescription("Sys.Delete")]
        [HttpPost]
        public async Task<ActionResult> Delete(Guid id, IFormCollection nouse)
        {
            var vm = Wtm.CreateVM<FrameworkUserVM>(id);
            await vm.DoDeleteAsync();
            if (!ModelState.IsValid)
            {
                return PartialView(vm);
            }
            else
            {
                return FFResult().CloseDialog().RefreshGrid();
            }
        }
        #endregion

        #region 详细
        [ActionDescription("Sys.Details")]
        public PartialViewResult Details(Guid id)
        {
            var v = Wtm.CreateVM<FrameworkUserVM>(id);
            return PartialView("Details", v);
        }
        #endregion

        #region BatchEdit
        [HttpPost]
        [ActionDescription("Sys.BatchEdit")]
        public ActionResult BatchEdit(string[] IDs)
        {
            var vm = Wtm.CreateVM<FrameworkUserBatchVM>(Ids: IDs);
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.BatchEdit")]
        public ActionResult DoBatchEdit(FrameworkUserBatchVM vm, IFormCollection nouse)
        {
            if (!ModelState.IsValid || !vm.DoBatchEdit())
            {
                return PartialView("BatchEdit", vm);
            }
            else
            {
                return FFResult().CloseDialog().RefreshGrid().Alert(Localizer["Sys.BatchEditSuccess", vm.Ids.Length]);
            }
        }
        #endregion


        #region 批量删除
        [HttpPost]
        [ActionDescription("Sys.BatchDelete")]
        public ActionResult BatchDelete(string[] IDs)
        {
            var vm = Wtm.CreateVM<FrameworkUserBatchVM>(Ids: IDs);
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.BatchDelete")]
        public async Task<ActionResult> DoBatchDelete(FrameworkUserBatchVM vm, IFormCollection nouse)
        {
            if (!ModelState.IsValid || !vm.DoBatchDelete())
            {
                return PartialView("BatchDelete", vm);
            }
            else
            {
                List<Guid?> tempids = new List<Guid?>();
                foreach (var item in vm?.Ids)
                {
                    tempids.Add(Guid.Parse(item));
                }
                var userids = DC.Set<FrameworkUserBase>().Where(x => tempids.Contains(x.ID)).Select(x => x.ID.ToString()).ToArray();
                await Wtm.RemoveUserCache(userids);
                return FFResult().CloseDialog().RefreshGrid().Alert(Localizer["Sys.OprationSuccess"]);
            }
        }
        #endregion

        #region 导入
        [ActionDescription("Sys.Import")]
        public ActionResult Import()
        {
            var vm = Wtm.CreateVM<FrameworkUserImportVM>();
            return PartialView(vm);
        }

        [HttpPost]
        [ActionDescription("Sys.Import")]
        public ActionResult Import(FrameworkUserImportVM vm, IFormCollection nouse)
        {
            if (vm.ErrorListVM.EntityList.Count > 0 || !vm.BatchSaveData())
            {
                return PartialView(vm);
            }
            else
            {
                return FFResult().CloseDialog().RefreshGrid().Alert(Localizer["Sys.ImportSuccess", vm.EntityList.Count.ToString()]);
            }
        }
        #endregion

        [ActionDescription("Sys.Enable")]
        public ActionResult Enable(Guid id, bool enable)
        {
            FrameworkUser user = new FrameworkUser { ID = id };
            user.IsValid = enable;
            DC.UpdateProperty(user, x => x.IsValid);
            DC.SaveChanges();
            return FFResult().RefreshGrid(CurrentWindowId);
        }

        [AllRights]
        public ActionResult GetUserById(string keywords)
        {
            var users = DC.Set<FrameworkUserBase>().Where(x => x.ITCode.ToLower().StartsWith(keywords.ToLower())).GetSelectListItems(Wtm, x=>x.Name +"("+x.ITCode+")", x => x.ITCode);
            return JsonMore(users);

        }

        [ActionDescription("Sys.Export")]
        [HttpPost]
        public IActionResult ExportExcel(FrameworkUserListVM vm)
        {
            return vm.GetExportData();
        }
    }
}
