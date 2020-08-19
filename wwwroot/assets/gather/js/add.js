var $url = '/gather/add';
var $urlActionsAttributes = '/gather/add/actions/attributes';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  ruleId: utils.getQueryInt('ruleId'),
  rule: null,
  channels: null,
  charsetList: null,
  attributes: null,
  form: null,
  step: 0
});

var methods = {
  insert: function(ref, text) {
    var tArea = document.getElementById(ref);
    var startPos = tArea.selectionStart;
    var endPos = tArea.selectionEnd;
    var tmpStr = this.form[ref] || '';
    this.form[ref] = tmpStr.substring(0, startPos) + text + tmpStr.substring(endPos, tmpStr.length);
  },

  getStartName: function(attribute) {
    return (attribute.value + 'Start').toLowerCase();
  },

  getEndName: function(attribute) {
    return (attribute.value + 'End').toLowerCase();
  },

  getDefaultName: function(attribute) {
    return (attribute.value + 'Default').toLowerCase();
  },

  apiGet: function () {
    var $this = this;

    $api.get($url, {
      params: {
        siteId: this.siteId,
        ruleId: this.ruleId
      }
    }).then(function (response) {
      var res = response.data;

      $this.rule = res.rule;
      $this.channels = res.channels;
      $this.charsetList = res.charsetList;
      $this.form = _.assign({
        urlInclude: '',
        channelIds: [],
        contentHtmlClearList: res.contentHtmlClearList,
        contentHtmlClearTagList: res.contentHtmlClearTagList,
      }, res.attributesDict, $this.rule);
      if ($this.rule.channelId) {
        $this.form.channelIds = [$this.rule.channelId];
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiAttributes: function () {
    var $this = this;

    this.form.channelId = 0;
    var obj = this.form.channelIds[0];
    if (Array.isArray(obj)) {
      this.form.channelId = obj[obj.length - 1];
    } else {
      this.form.channelId = obj;
    }

    utils.loading(this, true);
    $api.post($urlActionsAttributes, {
      siteId: this.siteId,
      channelId: this.form.channelId,
      ruleId: this.ruleId
    }).then(function (response) {
      var res = response.data;

      $this.attributes = res.attributes;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    this.form.contentAttributeList = [];
    for (var i = 0; i < this.attributes.length; i++) {
      var attribute = this.attributes[i];
      if (attribute.selected) {
        this.form.contentAttributeList.push(attribute.value);
      }
    }

    utils.loading(this, true);
    $api.post($url, this.form).then(function (response) {
      var res = response.data;

      $this.step = 5;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnPreviousClick: function () {
    if (this.step > 0) {
      this.step--;
      utils.scrollTop();
    }
  },

  btnSubmitClick: function () {
    var $this = this;
    this.$refs['form' + this.step].validate(function(valid) {
      if (valid) {
        if ($this.step === 4) {
          $this.apiSubmit();
          return;
        }
        
        if ($this.step === 1) {
          if ($this.form.gatherUrlIsSerialize && $this.form.gatherUrlSerialize.indexOf('*') === -1) {
            utils.error('序列相似网址必须包含 * 通配符');
            return;
          }
        }
        else if ($this.step === 2) {
          $this.apiAttributes();
        }

        $this.step++;
        utils.scrollTop();
      }
    });
  },

  btnCloseClick: function () {
    utils.removeTab();
  },
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});
