using Dapper;
using SuperNAT.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Common.Bll
{
    public class MapBll : BaseBll
    {
        public ReturnResult<bool> Add(Map model)
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

        public ReturnResult<bool> Update(Map model)
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

        public ReturnResult<bool> Delete(Map model)
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

        public ReturnResult<Map> GetOne(Map model)
        {
            var rst = new ReturnResult<Map>();

            try
            {
                rst.Data = conn.Get<Map>(model.id);
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

        public ReturnResult<List<Map>> GetList(Map model)
        {
            var rst = new ReturnResult<List<Map>>();

            try
            {
                rst.Data = conn.GetList<Map>("", model).ToList();
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
