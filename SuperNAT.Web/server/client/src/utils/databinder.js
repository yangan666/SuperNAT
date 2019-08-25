import DataBinder from "@icedesign/data-binder";
import { Message } from "@alifd/next";
import request from "@/utils/request";
import {
  setJwtToken,
  getJwtToken,
  setUserId,
  getUserId,
  removeToken
} from "@/utils/auth";
import React from "react";
import {
  FormBinderWrapper as IceFormBinderWrapper,
  FormBinder as IceFormBinder,
  FormError as IceFormError
} from "@icedesign/form-binder";
import IceIcon from "@icedesign/foundation-symbol";
import { Button, Dialog, Input } from "@alifd/next";
import styles from "./index.module.scss";

var show = false;
var formData = {
  user_id: "",
  user_name: "",
  password: ""
};
const handleSubmit = e => {
  e.preventDefault();
  request({
    url: "/Api/User/Login",
    method: "POST",
    data: formData
  }).then(({ data }) => {
    if (data.status == "SUCCESS") {
      Message.success(data.message);
      //记录到cookies
      setJwtToken(data.data.dataSource.token);
      setUserId(data.data.dataSource.user_id);
      //刷新页面
      location.reload();
    } else {
      Message.error(data.message);
    }
  });
};
const formChange = value => {
  formData.user_name = value.user_name;
  formData.password = value.password;
};
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
        responseHandler(body, response);
      },
      success: (body, defaultCallback, originResponse) => {
        if (body.status !== "SUCCESS") {
          // 后端返回的状态码错误
          if (body.code == 10005) {
            console.log("后端返回的状态码错误10005", body);
            Message.error(body.message);
            location.reload();
            return;
          }
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
        if (config.showErrorToast) {
          if (originResponse.status === 401) {
            var data = originResponse.data;
            //10000 未授权
            //10001 签名不正确
            //10002 会话超时
            //10003 授权验证失败
            if (data.Status === 10002) {
              //输入密码重新登录
              var user_id = getUserId();
              formData.user_id = user_id; //获取id
            } else {
              //跳转登录页面
              removeToken();
            }
            if (show) {
              return;
            }
            Message.error(data.Message);
            show = true;
            const dialog = Dialog.show({
              title: `请重新登录`,
              content: (
                <div className={styles.container}>
                  <IceFormBinderWrapper value={formData} onChange={formChange}>
                    <div className={styles.formItems}>
                      {(formData.user_id == "" ? true : false) && (
                        <div className={styles.formItem}>
                          <IceIcon
                            type="person"
                            size="small"
                            className={styles.inputIcon}
                          />
                          <IceFormBinder
                            name="user_name"
                            required
                            message="必填"
                          >
                            <Input
                              size="large"
                              placeholder="用户名"
                              className={styles.inputCol}
                            />
                          </IceFormBinder>
                          <IceFormError name="user_name" />
                        </div>
                      )}

                      <div className={styles.formItem}>
                        <IceIcon
                          type="lock"
                          size="small"
                          className={styles.inputIcon}
                        />
                        <IceFormBinder name="password" required message="必填">
                          <Input
                            size="large"
                            htmlType="password"
                            placeholder="密码"
                            className={styles.inputCol}
                          />
                        </IceFormBinder>
                        <IceFormError name="password" />
                      </div>
                    </div>
                  </IceFormBinderWrapper>
                </div>
              ),
              onClose: () => {
                show = false;
                formData = {};
              },
              footer: (
                <div className={styles.footer}>
                  <Button
                    type="primary"
                    size="large"
                    onClick={handleSubmit}
                    className={styles.submitBtn}
                  >
                    登 录
                  </Button>
                </div>
              )
            });
          } else {
            Message.error(err.message);
          }
        }
      }
    };
  });
  return DataBinder.call(this, newOptions, { requestClient: request });
};

export default CustomDataBinder;
