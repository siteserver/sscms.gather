var $url = '/gather/start';
var $urlActionsGather = '/gather/start/actions/gather';
var $urlActionsStatus = '/gather/start/actions/status';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleId: utils.getQueryInt('ruleId'),
  rule: null,
  form: {
    channelIds: null,
    channelId: null,
    gatherNum: null,
    isChecked: null,
    gatherUrlIsCollection: null,
    gatherUrlIsSerialize: null,
    gatherUrlCollection: null,
    gatherUrlSerialize: null,
    serializeFrom: null,
    serializeTo: null,
    serializeInterval: null,
    serializeIsOrderByDesc: null,
    serializeIsAddZero: null
  },
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
      $this.form = {
        channelIds: res.channelIds,
        channelId: res.rule.channelId,
        gatherNum: res.rule.gatherNum,
        isChecked: res.rule.isChecked,
        gatherUrlIsCollection: res.rule.gatherUrlIsCollection,
        gatherUrlIsSerialize: res.rule.gatherUrlIsSerialize,
        gatherUrlCollection: res.rule.gatherUrlCollection,
        gatherUrlSerialize: res.rule.gatherUrlSerialize,
        serializeFrom: res.rule.serializeFrom,
        serializeTo: res.rule.serializeTo,
        serializeInterval: res.rule.serializeInterval,
        serializeIsOrderByDesc: res.rule.serializeIsOrderByDesc,
        serializeIsAddZero: res.rule.serializeIsAddZero
      };
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    this.form.channelId = this.form.channelIds[this.form.channelIds.length - 1];

    utils.loading(this, true);
    $api.post($url, {
      siteId: this.siteId,
      ruleId: this.ruleId,
      channelId: this.form.channelId,
      gatherNum: this.form.gatherNum,
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

      location.href = utils.getPageUrl('gather', 'status', {
        siteId: $this.siteId,
        ruleIds: $this.ruleId
      });
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
