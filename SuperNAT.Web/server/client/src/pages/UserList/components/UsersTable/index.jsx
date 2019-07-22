import React from 'react';
import { Input, Table, Dialog } from '@alifd/next';
import styles from './index.module.scss';

export default function UsersTable({ data }) {
  const handleEdit = () => {
    Dialog.confirm({
      title: '提示',
      content: '只有管理员权限才能编辑',
    });
  };

  const handleDisabled = () => {
    Dialog.confirm({
      title: '提示',
      content: '只有管理员权限才能禁用',
    });
  };

  const handleDelete = () => {
    Dialog.confirm({
      title: '提示',
      content: '只有管理员权限才能删除',
    });
  };

  const renderOper = () => {
    return (
      <div className={styles.oper}>
        <a className={styles.button} onClick={handleEdit}>
          编辑
        </a>
        <a className={styles.button} onClick={handleDisabled}>
          禁用
        </a>
        <a className={styles.button} onClick={handleDelete}>
          删除
        </a>
      </div>
    );
  };
  return (
    <div>
      <div className={styles.searchBar}>
        <div className={styles.info}>总共 {data.length} 个用户</div>
        <Input
          style={{ width: '300px' }}
          placeholder="请输入关键字"
        />
      </div>
      <Table hasBorder={false} dataSource={data}>
        <Table.Column title="用户名" dataIndex="user_name" />
        <Table.Column title="微信号" dataIndex="wechat" />
        <Table.Column title="手机号码" dataIndex="tel" />
        <Table.Column title="状态" dataIndex="is_disabled" />
        <Table.Column title="操作" cell={renderOper} />
      </Table>
    </div>
  );
}
