using Dapper;
using SuperNAT.Dal;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNAT.Bll
{
    public class MapBll
    {
        private MapDal mapDal = new MapDal();

        public ApiResult<bool> Add(Map model)
        {
            var rst = new ApiResult<bool>();

            try
            {
                using Trans t = new Trans();
                var exitRemote = mapDal.IsExit(model, t);
                if (exitRemote.Result)
                {
                    rst.Message = "外网地址已被使用，请更换其它地址";
                    return rst;
                }

                rst = mapDal.Add(model, t);
                t.Commit();
            }
            catch (Exception ex)
            {
                Log4netUtil.Error($"添加映射失败：{ex}");
            }

            return rst;
        }

        public ApiResult<bool> Update(Map model)
        {
            var rst = new ApiResult<bool>();

            try
            {
                using Trans t = new Trans();
                var exitRemote = mapDal.IsExit(model, t);
                if (exitRemote.Result)
                {
                    rst.Message = "外网地址已被使用，请更换其它地址";
                    return rst;
                }

                rst = mapDal.Update(model, t);
                t.Commit();
            }
            catch (Exception ex)
            {
                Log4netUtil.Error($"更新映射失败：{ex}");
            }

            return rst;
        }

        public ApiResult<bool> Delete(Map model)
        {
            using (mapDal)
            {
                return mapDal.Delete(model);
            }
        }

        public ApiResult<Map> GetOne(Map model)
        {
            using (mapDal)
            {
                return mapDal.GetOne(model);
            }
        }

        public ApiResult<List<Map>> GetList(string where)
        {
            using (mapDal)
            {
                return mapDal.GetList(where);
            }
        }

        public ApiResult<List<Map>> GetList(Map model)
        {
            using (mapDal)
            {
                return mapDal.GetList(model);
            }
        }

        public ApiResult<List<Map>> GetMapList(string secret)
        {
            using (mapDal)
            {
                return mapDal.GetMapList(secret);
            }
        }
    }
}
