import React, { Component } from 'react';
import { Dialog, Input, Select, Upload, Button } from '@alifd/next';
import { FormBinderWrapper, FormBinder, FormError } from '@icedesign/form-binder';
import styles from './index.module.scss';

export default class MapDialog extends Component {
    constructor(props) {
        super(props);
        this.state = { formData: {}, clientList: [], clientOptions: [] };
    }
    componentWillReceiveProps(nextProps) {
        if (nextProps.formData !== this.state.formData) {
            this.setState({ formData: nextProps.formData })
            var newList = this.state.clientList.filter(c => c.user_id === nextProps.formData.user_id) || []
            const newOptions = newList.map(v => {
                return {
                    value: v.id,
                    label: v.name
                }
            })
            console.log('newOptions', newOptions)
            this.setState({ clientOptions: newOptions })
        }
        console.log('nextProps.clientList', nextProps.clientList)
        if (nextProps.clientList !== this.state.clientList) {
            this.setState({ clientList: nextProps.clientList })
        }
    }
    render() {
        const { dialogTitle, dialogVisible, getFormValue, setVisible, protocolList, userList } = this.props;
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
        const selectUserChange = (value, actionType, item) => {
            console.log('selectUserChange', value, actionType, item)
            if (!value && typeof value === undefined) {
                this.setState({ clientOptions: [] })
            } else {
                var newList = this.state.clientList.filter(c => c.user_id === value) || []
                const newOptions = newList.map(v => {
                    return {
                        value: v.id,
                        label: v.name
                    }
                })
                console.log('newOptions', newOptions)
                this.setState({ clientOptions: newOptions })
            }
            if (this.state.clientOptions.length === 0) {
                let data = Object.assign({}, this.state.formData, { client_id: '' })
                this.setState({
                    formData: data
                })
            }
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
                                        onChange={selectUserChange}
                                        className={styles.formWidth}
                                    />
                                </FormBinder>
                                <FormError className={styles.formError} name="user_id" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>选择主机</div>
                                <FormBinder name="client_id" required message="请选择正确的主机">
                                    <Select
                                        hasClear
                                        dataSource={this.state.clientOptions}
                                        placeholder="请选择主机"
                                        autoWidth={false}
                                        className={styles.formWidth}
                                    />
                                </FormBinder>
                                <FormError className={styles.formError} name="client_id" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>应用名称</div>
                                <FormBinder name="name" required message="请输入正确的应用名称">
                                    <Input hasClear placeholder="请输入应用名称" className={styles.formWidth} />
                                </FormBinder>
                                <FormError className={styles.formError} name="name" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>内网地址</div>
                                <FormBinder name="local" required message="请输入正确的内网地址">
                                    <Input hasClear placeholder="ip:port的格式，例如：192.168.0.88:8888" className={styles.formWidth} />
                                </FormBinder>
                                <FormError className={styles.formError} name="local" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>外网地址</div>
                                <FormBinder name="remote" required message="请输入正确的外网地址">
                                    <Input hasClear placeholder="ip/域名:port的格式，例如：test.supernat.cn:8888" className={styles.formWidth} />
                                </FormBinder>
                                <FormError className={styles.formError} name="remote" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formRequired}>协议类型</div>
                                <FormBinder name="protocol" required message="请选择正确的协议类型">
                                    <Select
                                        hasClear
                                        dataSource={[
                                            {
                                                value: 'http',
                                                label: 'http',
                                            },
                                            {
                                                value: 'https',
                                                label: 'https',
                                            }
                                        ]}
                                        placeholder="请选择协议类型"
                                        autoWidth={false}
                                        className={styles.formWidth}
                                    />
                                </FormBinder>
                                <FormError className={styles.formError} name="protocol" />
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formLabel}>加密类型</div>
                                <FormBinder name="ssl_type">
                                    <Select
                                        hasClear
                                        dataSource={protocolList}
                                        placeholder="请选择加密类型"
                                        autoWidth={false}
                                        className={styles.formWidth}
                                    />
                                </FormBinder>
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formLabel}>加密证书</div>
                                <FormBinder name="certfile">
                                    <Input hasClear placeholder="请上传证书" className={styles.formWidth} addonAfter={<Upload
                                        action="http://localhost:8088/Api/Common/Upload?name=CertFile"
                                        withCredentials={false}
                                        onSuccess={onUploadSuccess}
                                        defaultValue={[]}
                                        className={styles.addonAfterMargin}
                                    >
                                        <Button type="normal" style={{ margin: '0 0 10px' }}>上传</Button>
                                    </Upload>} />
                                </FormBinder>
                            </div>
                            <div className={styles.formItem}>
                                <div className={styles.formLabel}>证书密码</div>
                                <FormBinder name="certpwd">
                                    <Input hasClear placeholder="请输入证书密码" htmlType="password" className={styles.formWidth} />
                                </FormBinder>
                            </div>
                        </div>
                    </FormBinderWrapper>
                </Dialog>
            </div>
        );
    }
}
