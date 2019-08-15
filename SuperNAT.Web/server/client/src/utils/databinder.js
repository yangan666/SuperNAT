import DataBinder from "@icedesign/data-binder";
import { Message } from "@alifd/next";
import request from "@/utils/request";

/**
 * 自定义一个 DataBinder，建议放在 src/components/DataBinder 下。支持以下特性：
 *   - 通过 showSuccessToast/showErrorToast 配置是否要弹 toast
 *   - 通过 responseFormatter 格式化后端接口
 *
 */

const CustomDataBinder = options => {
  // 重组 options
  let newOptions = {};

  Object.keys(options).forEach(dataSourceKey => {
    var config = options[dataSourceKey];
    const { showErrorToast = true, showSuccessToast = true } = config;
    config = { ...config, showErrorToast, showSuccessToast };
    newOptions[dataSourceKey] = {
      ...config,
      responseFormatter: (responseHandler, body, response) => {
        if (body.code === 10000) {
          // token过期
          Message.error("会话过期，重新输入密码！");
          return;
        }

        responseHandler(body, response);
      },
      success: (body, defaultCallback, originResponse) => {
        const { config } = originResponse;
        if (body.status !== "SUCCESS") {
          // 后端返回的状态码错误
          if (config.showErrorToast) {
            Message.error(body.message);
          }
        } else {
          if (showSuccessToast) {
            Message.success(body.message);
          }
        }
      },
      error: (originResponse, defaultCallback, err) => {
        // 网络异常：404，302 等
        const { config } = originResponse;
        if (config.showErrorToast) {
          Message.error(err.message);
        }
      }
    };
  });
  return DataBinder.call(this, newOptions, { requestClient: request });
};

export default CustomDataBinder;
