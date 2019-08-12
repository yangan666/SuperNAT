import React, { Component } from 'react';
import TopBar from '@/components/TopBar';
import GeneralDialog from '@/components/GeneralDialog';
import UsersTable from './components/UsersTable';
import DataBinder from '@icedesign/data-binder';
import request from '@/utils/request'

@DataBinder({
  userData: {
    url: '/Api/User/GetList',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    }
  }
}, { requestClient: request })
export default class UserList extends Component {
  componentDidMount() {
    // 组件加载时获取数据源，数据获取完成会触发组件 render
    this.props.updateBindingData('userData', {
      data: {
        user_name: '123'
      }
    });
  }
  render() {
    const { userData } = this.props.bindingData
    const tableData = userData.dataSource
    const getFormValue = (value) => {
      tableData.push(value)
    };
    return (
      <div>
        <TopBar
          title="用户管理"
          extraAfter={<GeneralDialog buttonText="新建用户" getFormValue={getFormValue} />}
        />
        <UsersTable data={tableData} />
      </div>
    );
  }
}
