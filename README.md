# uta-tap

一起来制作 Mikutap 页面吧！

## 已部署站点

+ Cloudflare Pages: https://uta-tap.pages.dev/
+ Vercel (较快): https://vercel.uta.mrxiaom.top/
+ Github Pages (较慢): https://uta.mrxiaom.top/

## 这是什么

歌tap (uta-tap) 是可供用户编辑的 Mikutap 二次开发版本，计划做到以下内容
+ [x] 可载入自定义配置文件
+ [x] 提供背景音轨[可视化编辑器](https://github.com/MrXiaoM/uta-tap/releases/latest)
+ [x] 提供[utau工程](vocal.mid)以便于生成歌姬配置文件
  <details>
    <summary>使用 midi 的原因</summary>
    <p>综合考虑下来，还是 midi 的普遍适用性最强，只需导入 midi 然后编辑歌词就行了，歌词如下<p>
    <p><code>mo gu no ka te mo so o ga na ra pa ma ye de ro re to za ba sa ge nya sa ya bi te ha ko a tsu to</code></p>
    <p>当然你也可以使用你想要的歌词，自由发挥吧 xwx</p>
  </details>

我们的目标是使得不具备任何编程知识、计算机专业知识的用户也可以编辑 Mikutap 的人声、背景音轨等等。

![](https://pic1.imgdb.cn/item/68d7dd04c5157e1a883d0343.png)  
![](https://pic1.imgdb.cn/item/68d7dd02c5157e1a883d0336.png)

## 用法

到 [data](https://github.com/MrXiaoM/uta-tap/tree/main/data) 目录下载示例的音轨或者歌姬配置文件，根据文档进行编辑，最后在 [uta-tap 网页](https://vercel.uta.mrxiaom.top/) 的`:选择:`菜单上传自定义配置即可使用。

站点是静态网页，一切操作均在你的浏览器进行，不会上传到互联网。

## 鸣谢

+ [HFIProgramming 全体开发者](https://github.com/HFIProgramming) 制作 Mikutap 汉化
+ [daniwell](https://aidn.jp/) Mikutap 原作者
