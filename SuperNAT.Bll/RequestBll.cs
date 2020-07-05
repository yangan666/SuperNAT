using SuperNAT.Dal;
using SuperNAT.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNAT.Bll
{
    public class RequestBll
    {
        /// <summary>
        /// 数据访问层
        /// </summary>
        private readonly RequestDal requestDal = new RequestDal();

        /// <summary>
        /// 增加一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ApiResult<bool> Add(Request model)
        {
            try
            {
                using (requestDal)
                {
                    return requestDal.Add(model);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 增加多条数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public ApiResult<bool> AddList(List<Request> list)
        {
            try
            {
                using (requestDal)
                {
                    return requestDal.AddList(list);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 更新一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ApiResult<bool> Update(Request model)
        {
            try
            {
                using (requestDal)
                {
                    return requestDal.Update(model);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 删除一条数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ApiResult<bool> Delete(Request model)
        {
            try
            {
                using (requestDal)
                {
                    return requestDal.Delete(model);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<bool>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 得到一个对象实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ApiResult<Request> GetOne(Request model)
        {
            try
            {
                using (requestDal)
                {
                    return requestDal.GetOne(model);
                }
            }
            catch (Exception ex)
            {
                return ApiResult<Request>.Fail(ex.Message);
            }
        }

        /// <summary>
        /// 获得数据列表
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public ApiResult<List<Request>> GetList(Request model)
        {
            try
            {
                using (requestDal)
                {
                    return requestDal.GetList("");
                }
            }
            catch (Exception ex)
            {
                return ApiResult<List<Request>>.Fail(ex.Message);
            }
        }
    }
}
