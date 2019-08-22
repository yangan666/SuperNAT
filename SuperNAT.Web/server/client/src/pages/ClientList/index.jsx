import React, { Component } from 'react';
import TopBar from '@/components/TopBar';
import ClientTable from './components/ClientTable';
import ClientDialog from './components/ClientDialog';
import CustomDataBinder from '@/utils/databinder'
import { Dialog, Button } from '@alifd/next';

@CustomDataBinder({
  clientList: {
    url: '/Api/Client/GetList',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    },
    showSuccessToast: false
  },
  clientData: {
    url: '/Api/Client/GetOne',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    },
    showSuccessToast: false
  },
  addClient: {
    url: '/Api/Client/Add',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  delClient: {
    url: '/Api/Client/Delete',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  userOptions: {
    url: '/Api/User/GetList',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    },
    showSuccessToast: false
  },
})
export default class ClientList extends Component {
  constructor(props) {
    super(props);
    this.state = { dialogVisible: false, client: {} };
  }
  componentDidMount() {
    // 组件加载时获取数据源，数据获取完成会触发组件 render
    this.props.updateBindingData('clientList', {
      data: {}
    });
    this.props.updateBindingData('userOptions', {
      data: {}
    });
  }
  render() {
    const { clientList,  userOptions } = this.props.bindingData
    const userList = userOptions.dataSource.map(v => {
      return {
        value: v.id,
        label: v.user_name
      }
    })
    const getFormValue = (value) => {
      //保存
      this.props.updateBindingData('addClient', {
        data: value
      }, (res) => {
        if (res.status == "SUCCESS") {
          this.props.updateBindingData('clientList', {
            data: {}
          });
        }
      });
    };
    const setVisible = (value) => {
      if (value) {
        //新建映射
        this.props.updateBindingData('clientData', {
          data: {}
        }, (res) => {
          this.setState({ client: res.data.dataSource })
        });
      }
      this.setState({ dialogVisible: value })
    }
    const operate = (type, record) => {
      switch (type) {
        case 'edit':
          this.props.updateBindingData('clientData', {
            data: { id: record.id }
          }, (res) => {
            this.setState({ client: res.data.dataSource || {}, dialogVisible: true })
          });
          break;
        case 'delete':
          Dialog.confirm({
            title: '提示',
            content: `确定删除主机"${record.name}"吗`,
            onOk: () => {
              this.props.updateBindingData('delClient', {
                data: { id: record.id }
              }, (res) => {
                if (res.status == "SUCCESS") {
                  this.props.updateBindingData('clientList', {
                    data: {}
                  });
                }
              });
            }
          });
          break;
      }
    };
    return (
      <div>
        <TopBar
          title="用户管理"
          extraAfter={<Button type="primary" onClick={() => { setVisible(true) }}>新建主机</Button>}
        />
        <ClientTable data={clientList} operate={operate} />
        <ClientDialog dialogTitle={this.state.client.id === 0 ? "新建主机" : "编辑主机"}
          dialogVisible={this.state.dialogVisible}
          getFormValue={getFormValue}
          setVisible={setVisible}
          formData={this.state.client}
          userList={userList} />
      </div>
    );
  }
}
