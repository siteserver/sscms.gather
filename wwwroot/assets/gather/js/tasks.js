var $url = "/gather/tasks"
var $urlDelete = $url + "/actions/delete";
var $urlEnable = $url + "/actions/enable";

var $defaultForm = {
  id: 0,
  title: '',
  description: '',
  taskInterval: '',
  every: 1,
  weeks: [],
  startDate: new Date(new Date().setTime(new Date().getTime() + 3600 * 1000)),
  isNoticeSuccess: false,
  isNoticeFailure: true,
  noticeFailureCount: 1,
  isNoticeMobile: false,
  noticeMobile: '',
  isNoticeMail: false,
  noticeMail: '',
  isDisabled: false,
  timeout: 30,
  siteId: utils.getQueryInt('siteId'),
  ruleIds: [],
};

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  isAdd: false,
  task: null,
  active: 1,
  form: _.assign({}, $defaultForm),
  cloudType: null,
  taskIntervals: null,
  tasks: null,
  rules: [],
});

var methods = {
  apiGet: function() {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId
      }
    }).then(function (response) {
      var res = response.data;

      $this.cloudType = res.cloudType;
      $this.taskIntervals = res.taskIntervals;
      $this.tasks = res.tasks;
      $this.rules = res.rules;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    utils.loading(this, true);
    $api.post($url, this.form).then(function (response) {
      var res = response.data;

      utils.success('定时任务创建成功！');
      $this.isAdd = false;
      $this.apiGet();
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnPreviousClick: function () {
    this.active--;
  },

  btnNextClick: function() {
    var $this = this;
    this.$refs.addForm.validate(function(valid) {
      if (valid) {
        if ($this.active == 3) {
          $this.apiSubmit();
        } else {
          $this.active++;
        }
      }
    });
  },

  btnCancelClick: function() {
    this.isAdd = false;
  },

  btnCloseClick: function() {
    utils.removeTab();
  },

  validateEmail: function (rule, value, callback) {
    if (!value) {
      callback(new Error('请输入通知邮箱'));
    } else {
      var re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
      if (!re.test(value))
      {
        callback(new Error('通知邮箱格式不正确'));
      } else {
        callback();
      }
    }
  },

  validateMobile: function (rule, value, callback) {
    if (!value) {
      callback(new Error('请输入通知手机号码'));
    } else if (!/^1[3-9]\d{9}$/.test(value)) {
      callback(new Error('手机号码格式不正确'));
    } else {
      callback()
    }
  },

  apiDelete: function (id) {
    var $this = this;

    utils.loading(this, true);
    $api.post($urlDelete, {
        siteId: this.siteId,
        id: id,
      })
      .then(function (response) {
        var res = response.data;

        utils.success("定时任务删除成功！");
        $this.apiGet();
      })
      .catch(function (error) {
        utils.error(error);
      })
      .then(function () {
        utils.loading($this, false);
      });
  },

  apiEnable: function (task) {
    var $this = this;

    utils.loading(this, true);
    $api.post($urlEnable, {
        siteId: this.siteId,
        id: task.id,
      })
      .then(function (response) {
        var res = response.data;

        var name = task.isDisabled ? '启用' : '禁用'
        utils.success("定时任务" + name + "成功！");
        $this.apiGet();
      })
      .catch(function (error) {
        utils.error(error);
      })
      .then(function () {
        utils.loading($this, false);
      });
  },

  getTaskInterval: function (taskInterval) {
    for (var item of this.taskIntervals) {
      if (item.value == taskInterval) {
        return item.label;
      }
    }
    return '';
  },

  btnUpgradeClick: function () {
    location.href = utils.getCloudsUrl('dashboard', {isUpgrade: true});
  },

  btnDeleteClick: function(task) {
    var $this = this;

    utils.alertDelete({
      title: '删除定时任务',
      text: '此操作将删除定时任务 “' + task.title + '”，确定吗？',
      callback: function () {
        $this.apiDelete(task.id);
      }
    });
  },

  btnEnableClick: function(task) {
    var $this = this;

    var name = task.isDisabled ? '启用' : '禁用';
    var button = task.isDisabled ? '启 用' : '禁 用';
    utils.alertDelete({
      title: name + '定时任务',
      text: '此操作将' + name + '定时任务 “' + task.title + '”，确定吗？',
      button: button,
      callback: function () {
        $this.apiEnable(task);
      }
    });
  },

  btnAddClick: function () {
    this.isAdd = true;
    this.task = null;
    this.active = 1;
    this.form = _.assign({}, $defaultForm);
  },

  btnEditClick: function(task) {
    this.isAdd = true;
    this.task = task;
    this.active = 1;
    this.form = _.assign({}, $defaultForm, task);
    if (!this.form.ruleIds) {
      this.form.ruleIds = [];
    }
  },
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    var $this = this;
    utils.keyPress(function () {
      if ($this.isAdd) {
        $this.btnNextClick();
      }
    }, function () {
      if ($this.isAdd) {
        $this.btnCancelClick();
      } else {
        $this.btnCloseClick();
      }
    });
    this.apiGet();
  }
});
