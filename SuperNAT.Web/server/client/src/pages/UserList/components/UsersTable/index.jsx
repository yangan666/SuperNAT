import React from 'react';
import { Input, Table } from '@alifd/next';
import styles from './index.module.scss';
import IceLabel from '@icedesign/label';

export default function UsersTable({ data, operate }) {
  const renderStatus = (value, index, record) => {
    return (
      <IceLabel status={record.is_disabled ? 'danger' : 'success'}>{record.is_disabled_str}</IceLabel>
    );
  }
  const renderOper = (value, index, record) => {
    return (
      <div className={styles.oper}>
        <a className={styles.button} onClick={() => { operate('edit', record) }}>
          编辑
        </a>
        <a className={styles.button} onClick={() => { operate('disable', record) }}>
          {record.is_disabled ? "启用" : "禁用"}
        </a>
        <a className={styles.button} onClick={() => { operate('delete', record) }}>
          删除
        </a>
      </div>
    );
  };
  const tableData = data.dataSource || []
  return (
    <div>
      <div className={styles.searchBar}>
        <div className={styles.info}>共 {tableData.length} 条记录</div>
        <Input
          style={{ width: '300px' }}
          placeholder="请输入关键字"
        />
      </div>
      <Table loading={data.__loading} hasBorder={false} dataSource={tableData}>
        <Table.Column width={180} title="用户名" dataIndex="user_name" />
        <Table.Column width={180} title="微信号" dataIndex="wechat" />
        <Table.Column width={200} title="手机号码" dataIndex="tel" />
        <Table.Column width={150} title="状态" cell={renderStatus} />
        <Table.Column title="操作" cell={renderOper} />
      </Table>
    </div>
  );
}
