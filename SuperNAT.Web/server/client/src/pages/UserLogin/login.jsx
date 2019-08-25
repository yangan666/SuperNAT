import React, { Component } from 'react';
import { Input } from '@alifd/next';
import {
    FormBinderWrapper as IceFormBinderWrapper,
    FormBinder as IceFormBinder,
    FormError as IceFormError,
} from '@icedesign/form-binder';
import IceIcon from '@icedesign/foundation-symbol';
import styles from './index.module.scss';
import CustomDataBinder from '@/utils/databinder'
import { setJwtToken, getJwtToken, setUserId, getUserId } from "@/utils/auth";

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
class Login extends Component {
    constructor(props) {
        super(props);
        this.state = {
            formData: {
                user_name: '',
                password: ''
            }
        };
    }
    componentDidMount() {
        var
        this.setState({ formData: value })
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
                    </div>
                </IceFormBinderWrapper>
            </div>
        );
    }
}

export default Login;
