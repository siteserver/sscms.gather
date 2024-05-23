var $url = '/gather/status';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleIds: utils.getQueryString('ruleIds'),
  rules: [],
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        ruleIds: this.ruleIds
      }
    }).then(function (response) {
      var res = response.data;

      $this.rules = res.rules;
      setTimeout(function () {
        $this.apiSubmit();
      }, 1000);
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function() {
    var $this = this;

    $api.post($url, {
      siteId: this.siteId,
      ruleIds: this.ruleIds
    }).then(function (response) {
      var res = response.data;

      var isProgress = false;
      for (var i = 0; i < res.caches.length; i++) {
        var rule = $this.rules[i];

        rule.cache = res.caches[i] || {};
        if (rule.cache.totalCount > 0) {
          rule.percentage = ((rule.cache.successCount/rule.cache.totalCount) * 100).toFixed(1);
        } else {
          rule.percentage = 0;
        }

        if (rule.cache.status === 'progress') {
          isProgress = true;
        }
      }

      if (isProgress) {
        setTimeout(function () {
          $this.apiSubmit();
        }, 1000);
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
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
