import React, { Component } from 'react';
import TopBar from '@/components/TopBar';
import GeneralDialog from '@/components/GeneralDialog';
import UsersTable from './components/UsersTable';
import DataBinder from '@icedesign/data-binder';

export default function UserList() {
  DataBinder({
    userData: {
      url: '/Api/User/GetList',
      method: 'POST',
      responseFormatter: (responseHandler, body, response) => {
        // 拿到接口返回的 res 数据，做一些格式转换处理，使其符合 DataBinder 的要求
        console.log('body', body)
        responseHandler(body, response)
      },
      defaultBindingData: {
        tableData: []
      }
    }
  })
  // const { userData } = this.DataBinder
  // const tableData = userData.tableData
  // const getFormValue = (value) => {
  //   tableData.push(value)
  // };
  const tableData = []
  return (
    <div>
      <TopBar
        title="用户管理"
        extraAfter={<GeneralDialog buttonText="新建用户" />}
      />
      <UsersTable data={tableData} />
    </div>
  );
}
