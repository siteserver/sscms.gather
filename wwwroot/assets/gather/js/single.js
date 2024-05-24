var $url = '/gather/single';
var $urlActionsStatus = '/gather/single/actions/status';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleId: utils.getQueryInt('ruleId'),
  pageProcess: false,
  rule: null,
  form: {
    channelIds: null,
    channelId: null,
    gatherNum: null,
    isChecked: null,
    gatherUrlIsCollection: true,
    gatherUrlIsSerialize: false,
    gatherUrlCollection: '',
    gatherUrlSerialize: '',
    serializeFrom: 1,
    serializeTo: 100,
    serializeInterval: 1,
    serializeIsOrderByDesc: false,
    serializeIsAddZero: false
  },
  guid: null,
  cache: {},
  percentage: null,
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        ruleId: this.ruleId
      }
    }).then(function (response) {
      var res = response.data;

      $this.rule = res.rule;
      $this.channels = res.channels;
      $this.form.channelIds = [];
      $this.form.channelId = res.rule.channelId;
      $this.form.isChecked = res.rule.isChecked;

      if ($this.rule.channelId) {
        $this.form.channelIds = [$this.rule.channelId];
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    this.form.channelId = 0;
    var obj = this.form.channelIds[0];
    if (Array.isArray(obj)) {
      this.form.channelId = obj[obj.length - 1];
    } else {
      this.form.channelId = obj;
    }

    utils.loading(this, true);
    $api.post($url, {
      siteId: this.siteId,
      ruleId: this.ruleId,
      channelId: this.form.channelId,
      isChecked: this.form.isChecked,
      gatherUrlIsCollection: this.form.gatherUrlIsCollection,
      gatherUrlIsSerialize: this.form.gatherUrlIsSerialize,
      gatherUrlCollection: this.form.gatherUrlCollection,
      gatherUrlSerialize: this.form.gatherUrlSerialize,
      serializeFrom: this.form.serializeFrom,
      serializeTo: this.form.serializeTo,
      serializeInterval: this.form.serializeInterval,
      serializeIsOrderByDesc: this.form.serializeIsOrderByDesc,
      serializeIsAddZero: this.form.serializeIsAddZero
    }).then(function (response) {
      var res = response.data;
      $this.guid = res.value;

      $this.pageProcess = true;
      $this.apiGetStatus();
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiGetStatus: function() {
    var $this = this;

    $api.post($urlActionsStatus, {
      siteId: this.siteId,
      guid: this.guid
    }).then(function (response) {
      var res = response.data;

      $this.cache = res.cache || {};
      if ($this.cache.totalCount > 0) {
        $this.percentage = (($this.cache.successCount/$this.cache.totalCount) * 100).toFixed(1);
      }else {
        $this.percentage = 0;
      }

      if ($this.cache.status === 'progress') {
        $this.apiGetStatus();
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnSubmitClick: function() {
    this.apiSubmit();
  },

  btnCloseClick: function () {
    utils.removeTab();
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});
