# 中华人民共和国县以上行政区划代码数据

数据更新日期：2020年10月

来源：中华人民共和国民政部官方网站（ http://www.mca.gov.cn ）

## 使用

[下载](https://github.com/stulzq/ChinaRegionCode/releases/latest) 文件json数据文件，然后放到 web server 静态资源目录即可。

`region_antd.json` 树形结构，适配 Ant Design 的级联选择组件。

`region_three.json` 扁平化数组，理论可以转换成任意结构。

**Code 为行政区划代码**

## 其它

爬取代码采用 C# 编写，你可以任意修改来实现你自己需求的数据格式。

如民政部官方网站有但更新未能及时提供，可以提交 [issues](https://github.com/stulzq/ChinaRegionCode/issues)。
