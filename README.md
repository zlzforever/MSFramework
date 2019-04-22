# MSFramework
A micro service template

### 领域事件

1. 如果是单体应用，事件总线使用内存直通即可
2. 如果各领域是微服务实例，必须保证每个聚合根分发到对应的处理实例上。

a. 所有事件先保存到数据库
b. 发送处理事件
c. 处理实例通过聚合根ID取到所有事件并处理

领域事件，可以用于一个限界上下文内的领域模型，也可以使用消息队列在限界上下文间进行异步通信。
领域事件的主要用途：

+ 保证聚合间的数据一致性
+ 替换批量处理
+ 实现事件源模式
+ 进行限界上下文集成


 
###

1. CQRS Command: MeditaR


###

1. Query 查询的聚合根数据要保留 Id, Version
2. UI 层发送 Command 修改聚合根，必须传递 Id, Version

       + 从 ES 中查询聚合根事件 Get(id, from version)
       + 如果没有事件说明前端数据和 ES数据一致，直接从数据库表中取得最新聚合根完整数据
       + 如果有事件，说明数据库表中聚合根数据不一致，则有两种可能：1种完全重构数据并替换，2 如果ES刚相差1个版本，即可以接续，则从ES数据中还原聚合根，并更新

3. 领域事件有两种: 本地事件(本地事件)、分布式事件(用于微服务各服务之间通信)