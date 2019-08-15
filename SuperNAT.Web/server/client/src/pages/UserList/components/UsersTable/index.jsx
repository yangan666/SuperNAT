import React from 'react';
import { Input, Table, Dialog } from '@alifd/next';
import styles from './index.module.scss';

export default function UsersTable({ data, operate }) {
  const renderOper = (value, index, record) => {
    return (
      <div className={styles.oper}>
        <a className={styles.button} onClick={() => { operate('edit', record) }}>
          编辑
        </a>
        <a className={styles.button} onClick={() => { operate('disable', record) }}>
          禁用
        </a>
        <a className={styles.button} onClick={() => { operate('delete', record) }}>
          删除
        </a>
      </div>
    );
  };
  return (
    <div>
      <div className={styles.searchBar}>
        <div className={styles.info}>共 {data.length} 条记录</div>
        <Input
          style={{ width: '300px' }}
          placeholder="请输入关键字"
        />
      </div>
      <Table hasBorder={false} dataSource={data}>
        <Table.Column title="用户名" dataIndex="user_name" />
        <Table.Column title="微信号" dataIndex="wechat" />
        <Table.Column title="手机号码" dataIndex="tel" />
        <Table.Column title="状态" dataIndex="is_disabled_str" />
        <Table.Column title="操作" cell={renderOper} />
      </Table>
    </div>
  );
}
