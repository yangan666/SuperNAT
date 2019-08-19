import React, { Component } from 'react';
import TopBar from '@/components/TopBar';
import MapTable from './components/MapTable';
import MapDialog from './components/MapDialog';
import CustomDataBinder from '@/utils/databinder'
import { Dialog, Button } from '@alifd/next';

@CustomDataBinder({
  mapList: {
    url: '/Api/Map/GetList',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    },
    showSuccessToast: false
  },
  mapData: {
    url: '/Api/Map/GetOne',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    },
    showSuccessToast: false
  },
  addMap: {
    url: '/Api/Map/Add',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  delMap: {
    url: '/Api/Map/Delete',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: {}
    }
  },
  protocolOptions: {
    url: '/Api/Common/GetEnumList?type=ssl_type',
    method: 'POST',
    data: {},
    defaultBindingData: {
      dataSource: []
    },
    showSuccessToast: false
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
export default class MapList extends Component {
  constructor(props) {
    super(props);
    this.state = { dialogVisible: false, map: {} };
  }
  componentDidMount() {
    // 组件加载时获取数据源，数据获取完成会触发组件 render
    this.props.updateBindingData('mapList', {
      data: {}
    });
    this.props.updateBindingData('protocolOptions', {
      data: {}
    });
    this.props.updateBindingData('userOptions', {
      data: {}
    });
  }
  render() {
    const { mapList, protocolOptions, userOptions } = this.props.bindingData
    const protocolList = protocolOptions.dataSource.map(v => {
      return {
        value: v.Key,
        label: v.Value
      }
    })
    const userList = userOptions.dataSource.map(v => {
      return {
        value: v.id,
        label: v.user_name
      }
    })
    const getFormValue = (value) => {
      //保存
      this.props.updateBindingData('addMap', {
        data: value
      }, (res) => {
        if (res.status == "SUCCESS") {
          this.props.updateBindingData('mapList', {
            data: {}
          });
        }
      });
    };
    const setVisible = (value) => {
      if (value) {
        //新建映射
        this.props.updateBindingData('mapData', {
          data: {}
        }, (res) => {
          this.setState({ map: res.data.dataSource })
        });
      }
      this.setState({ dialogVisible: value })
    }
    const operate = (type, record) => {
      switch (type) {
        case 'edit':
          this.props.updateBindingData('mapData', {
            data: { id: record.id }
          }, (res) => {
            this.setState({ map: res.data.dataSource || {}, dialogVisible: true })
          });
          break;
        case 'delete':
          Dialog.confirm({
            title: '提示',
            content: `确定删除映射"${record.name}"吗`,
            onOk: () => {
              this.props.updateBindingData('delMap', {
                data: { id: record.id }
              }, (res) => {
                if (res.status == "SUCCESS") {
                  this.props.updateBindingData('mapList', {
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
          extraAfter={<Button type="primary" onClick={() => { setVisible(true) }}>新建映射</Button>}
        />
        <MapTable data={mapList} operate={operate} />
        <MapDialog dialogTitle={this.state.map.id === 0 ? "新建映射" : "编辑映射"}
          dialogVisible={this.state.dialogVisible}
          getFormValue={getFormValue}
          setVisible={setVisible}
          formData={this.state.map}
          protocolList={protocolList}
          userList={userList} />
      </div>
    );
  }
}
