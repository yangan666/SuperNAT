import React, { Component } from 'react';
import { Dialog, Input, Select, Upload, Button } from '@alifd/next';
import { FormBinderWrapper, FormBinder, FormError } from '@icedesign/form-binder';
import styles from './index.module.scss';

export default class ClientDialog extends Component {
    constructor(props) {
        super(props);
        this.state = { formData: {} };
    }
    componentWillReceiveProps(nextProps) {
        if (nextProps.formData !== this.state.formData) {
            this.setState({ formData: nextProps.formData })
        }
    }
    render() {
        const { dialogTitle, dialogVisible, getFormValue, setVisible, userList } = this.props;
        const handleConfirm = () => {
            const formEl = this.refs.form;
            formEl.validateAll((error, value) => {
                if (error) {
                    return;
                }
                setVisible(false);
                getFormValue(value);
            });
        }
        const onUploadSuccess = (info) => {
            let data = Object.assign({}, this.state.formData, { certfile: info.response.Data })
            this.setState({
                formData: data
            })
        }

        return (
            <div>
                <Dialog
                    visible={dialogVisible}
                    onOk={handleConfirm}
                    closeable="esc,close"
                    onCancel={() => setVisible(false)}
                    onClose={() => setVisible(false)}
                    title={dialogTitle}
                >
                    <FormBinderWrapper ref="form" value={this.state.formData}>
                        <div className={styles.formContent}>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>所属用户</div>
                                <FormBinder name="user_id" required message="请选择正确的所属用户">
                                    <Select
                                        hasClear
                                        dataSource={userList}
                                        placeholder="请选择所属用户"
                                        autoWidth={false}
                                        className={styles.formWidth}
                                    />
                                </FormBinder>
                                <FormError className={styles.formError} name="user_id" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>主机名称</div>
                                <FormBinder name="name" required message="请输入正确的主机名称">
                                    <Input hasClear placeholder="请输入主机名称" className={styles.formWidth} />
                                </FormBinder>
                                <FormError className={styles.formError} name="name" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>二级域名</div>
                                <FormBinder name="subdomain" required message="请输入正确的二级域名">
                                    <Input hasClear placeholder="请输入二级域名" className={styles.formWidth} />
                                </FormBinder>
                                <FormError className={styles.formError} name="subdomain" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formLabel}>描述</div>
                                <FormBinder name="remark">
                                    <Input hasClear placeholder="请输入描述" className={styles.formWidth} />
                                </FormBinder>
                                <FormError className={styles.formError} name="remark" />
                            </div>
                        </div>
                    </FormBinderWrapper>
                </Dialog>
            </div>
        );
    }
}
