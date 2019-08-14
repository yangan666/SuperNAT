import React, { useState, useRef } from 'react';
import { Dialog, Input, Message, Switch } from '@alifd/next';
import {
    FormBinderWrapper as IceFormBinderWrapper,
    FormBinder as IceFormBinder,
} from '@icedesign/form-binder';
import styles from './index.module.scss';

export default function Card(props) {
    const formEl = useRef(null);
    const { dialogTitle, dialogVisible, getFormValue, setVisible, formData } = props;
    function handleConfirm() {
        formEl.current.validateAll((error, value) => {
            if (error) {
                Message.error('请输入完整的信息');
                return;
            }
            setVisible(false);
            getFormValue(value);
        });
    }

    return (
        <div>
            <Dialog
                visible={dialogVisible}
                onOk={handleConfirm}
                closeable="esc,mask,close"
                onCancel={() => setVisible(false)}
                onClose={() => setVisible(false)}
                title={dialogTitle}
            >
                <IceFormBinderWrapper ref={formEl} value={formData}>
                    <div className={styles.formContent}>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>用户名</div>
                            <IceFormBinder required>
                                <Input name="user_name" style={{ width: '400px' }} />
                            </IceFormBinder>
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>密码</div>
                            <IceFormBinder>
                                <Input htmlType="password" name="password" style={{ width: '400px' }} />
                            </IceFormBinder>
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>微信</div>
                            <IceFormBinder required>
                                <Input name="wechat" style={{ width: '400px' }} />
                            </IceFormBinder>
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>手机</div>
                            <IceFormBinder required>
                                <Input name="tel" style={{ width: '400px' }} />
                            </IceFormBinder>
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>禁用</div>
                            <IceFormBinder>
                                <Switch name="is_disabled" />
                            </IceFormBinder>
                        </div>
                    </div>
                </IceFormBinderWrapper>
            </Dialog>
        </div>
    );
}
