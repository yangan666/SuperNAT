using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class AppBll : BaseBll
    {
        public ReturnResult<bool> Add(App model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                rst.Result = conn.Insert(model) > 0;
                rst.Message = "添加成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"添加失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<bool> Update(App model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                rst.Result = conn.Update(model) > 0;
                rst.Message = "更新成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"更新失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<bool> Delete(App model)
        {
            var rst = new ReturnResult<bool>();

            try
            {
                rst.Result = conn.Delete(model) > 0;
                rst.Message = "删除成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"删除失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<App> GetOne(App model)
        {
            var rst = new ReturnResult<App>();

            try
            {
                rst.Data = conn.Get<App>(model.id);
                rst.Result = true;
                rst.Message = "获取成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"获取失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }

        public ReturnResult<List<App>> GetList(App model)
        {
            var rst = new ReturnResult<List<App>>();

            try
            {
                rst.Data = conn.GetList<App>("", model).ToList();
                rst.Result = true;
                rst.Message = "获取成功";
            }
            catch (Exception ex)
            {
                rst.Message = $"获取失败：{ex.InnerException ?? ex}";
                Log4netUtil.Error($"{ex.InnerException ?? ex}");
            }

            return rst;
        }
    }
}
