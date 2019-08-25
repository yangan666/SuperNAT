import React, { Component } from 'react';
import { withRouter, Link } from 'react-router-dom';
import { Input, Button, Checkbox, Message } from '@alifd/next';
import {
  FormBinderWrapper as IceFormBinderWrapper,
  FormBinder as IceFormBinder,
  FormError as IceFormError,
} from '@icedesign/form-binder';
import IceIcon from '@icedesign/foundation-symbol';
import styles from './index.module.scss';
import CustomDataBinder from '@/utils/databinder'
import { setJwtToken, getJwtToken, setUserId } from "@/utils/auth";

@CustomDataBinder({
  login: {
    url: '/Api/User/Login',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  }
})
class UserLogin extends Component {
  constructor(props) {
    super(props);
    this.state = {
      formData: {
        user_name: '',
        password: '',
        checkbox: false
      }
    };
  }
  componentDidMount() {
    
  }
  render() {
    const formEl = this.refs.form;

    const formChange = (value) => {
      this.setState({ formData: value })
    };

    const handleSubmit = (e) => {
      e.preventDefault();
      formEl.validateAll((errors, values) => {
        if (errors) {
          console.log('errors', errors);
          return;
        }
        this.props.updateBindingData('login', {
          data: this.state.formData
        }, res => {
          console.log('login', res);
          if (res.status === 'SUCCESS') {
            var data = res.data.dataSource
            //记录到cookies
            setJwtToken(data.token);
            setUserId(data.user_id);
            //跳转到主页
            this.props.history.push('/');
          }
        });
      });
    };

    return (
      <div className={styles.container}>
        <h4 className={styles.title}>登 录</h4>
        <IceFormBinderWrapper
          value={this.state.formData}
          onChange={formChange}
          ref="form"
        >
          <div className={styles.formItems}>
            <div className={styles.formItem}>
              <IceIcon type="person" size="small" className={styles.inputIcon} />
              <IceFormBinder name="user_name" required message="必填">
                <Input
                  size="large"
                  placeholder="用户名"
                  className={styles.inputCol}
                />
              </IceFormBinder>
              <IceFormError name="user_name" />
            </div>

            <div className={styles.formItem}>
              <IceIcon type="lock" size="small" className={styles.inputIcon} />
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

            <div className={styles.formItem}>
              <IceFormBinder name="checkbox">
                <Checkbox className={styles.checkbox}>记住账号</Checkbox>
              </IceFormBinder>
            </div>

            <div className={styles.footer}>
              <Button
                type="primary"
                size="large"
                onClick={handleSubmit}
                className={styles.submitBtn}
              >
                登 录
            </Button>
              <Link to="/user/register" className={styles.tips}>
                立即注册
            </Link>
            </div>
          </div>
        </IceFormBinderWrapper>
      </div>
    );
  }
}

export default withRouter(UserLogin);
