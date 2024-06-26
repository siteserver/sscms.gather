## 1.3.1
* 修复通过从相似网址采集功能只能采集199篇问题

## 1.3.0
* 兼容 SSCMS 7.3.0
* 新增单页采集通过从相似网址采集功能

## 1.2.4
* 对设置为已审核的采集规则，采集完成后将生成内容及栏目页面
* 新增定时任务功能，可添加定时执行采集任务 #3626 #3649
* 新增批量采集功能，可一键采集多个规则 #3721

## 1.2.3
* 兼容 SSCMS 7.2.2

## 1.2.2
* 修复将列表页中的图片设为封面图片环境下无法下载图片至本地问题

## 1.2.1
* 修复Redis缓存环境下采集不稳定问题

## 1.2.0
* 迁移至.NET 7

## 1.1.3
* 修复当图片路径包含../字符串时无法采集图片问题

## 1.1.2
* 修复无法从编码为gb2312的页面采集问题

## 1.1.1
* 兼容 SSCMS 7.1.1

## 1.1.0
* 迁移至.NET 6

## 1.0.16
* 更新API至最新版本

## 1.0.15
* 新增原内容页面文件名采集功能

## 1.0.14
* 修复图片没有加 a 链接，无法下载远程图片问题

## 1.0.13
* 修复接口兼容性

## 1.0.12
* 修复从列表采集日期功能

## 1.0.11
* 新增从列表页获取标题功能
* 新增从列表页获取简介功能
* 新增从列表页获取可选字段功能
* 修复默认值填充功能

## 1.0.10
* 新增列表页图片采集功能
* 调整内容地址获取规则
* 新增采集规则导入功能
* 新增采集规则导出功能

## 1.0.9
* 修复添加采集规则时服务保存采集栏目为下级栏目的情况

## 1.0.8
* 新增点击量采集

## 1.0.7
* 更换ICacheManager接口

## 1.0.6
* 点击添加采集规则时提示错误

## 1.0.5
* 更新插件文档
* 升级SSCMS依赖包至最新版本

## 1.0.4
* 采集缺少采集时间字段
* 采集保存栏目

## 1.0.3
* 解决采集设置已审核，但采集下来还是未审核问题

## 1.0.2
* IPlugin.Current引用导致不稳定，采用直接编码

## 1.0.1
* 引用 SSCMS 7.0.0 NuGet 正式版

## 1.0.0
* 从 .NET FRAMEWORK 迁移至 .NET CORE 平台