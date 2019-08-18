import React, { useRef } from 'react';
import { Dialog, Input } from '@alifd/next';
import { FormBinderWrapper, FormBinder, FormError } from '@icedesign/form-binder';
import styles from './index.module.scss';

export default function Card(props) {
    const formEl = useRef(null);
    const { dialogTitle, dialogVisible, getFormValue, setVisible, formData } = props;
    const handleConfirm = () => {
        formEl.current.validateAll((error, value) => {
            if (error) {
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
                <FormBinderWrapper ref={formEl} value={formData}>
                    <div className={styles.formContent}>
                        <div className={styles.formItem}>
                            <div className={styles.formRequired}>用户名</div>
                            <FormBinder name="user_name" required message="请输入正确的用户名">
                                <Input hasClear placeholder="请输入用户名" className={styles.formWidth} />
                            </FormBinder>
                            <FormError className={styles.formError} name="user_name" />
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>密码</div>
                            <FormBinder name="password">
                                <Input hasClear placeholder="请输入密码，不填写默认123456" htmlType="password" className={styles.formWidth} />
                            </FormBinder>
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formRequired}>手机</div>
                            <FormBinder name="tel" required message="请输入正确的手机">
                                <Input hasClear placeholder="请输入手机" className={styles.formWidth} />
                            </FormBinder>
                            <FormError className={styles.formError} name="tel" />
                        </div>
                        <div className={styles.formItem}>
                            <div className={styles.formLabel}>微信号</div>
                            <FormBinder name="wechat">
                                <Input hasClear placeholder="请输入微信号" className={styles.formWidth} />
                            </FormBinder>
                        </div>
                    </div>
                </FormBinderWrapper>
            </Dialog>
        </div>
    );
}
