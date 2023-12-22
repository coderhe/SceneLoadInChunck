# SceneLoadInChunck
a demo of scene loading/unloading in chuncks of Unity Engine.

# 场景分块加载功能说明
  分块加载说明: 有些场景过于大，因为场景加载时间过长。因此才会要到使用场景分块加载功能加速场景加载速度。基于此原因，采用以场景中的prefab为最小的加载单位，而不需要切割Mesh。
# 数据生成流程
  1.地形分块原理
  根据指定的正方形边长，将地形划分成不同的正方形区域，并对区域进行唯一的编号，加以区分。具体编号策略如图1所示。
![image](https://github.com/coderhe/SceneLoadInChunck/assets/3722873/cb4ac9f5-5c46-43fa-98d3-5591d0e86383)
  地块编号的规则是：由中心点，向外扩散的一个正方形。每一层的是数量，是一个等差数列。这样编辑场景块的好处是。位置和场景块索引能够相互转换，即指根据定位置能求出对应的场景块索引，指定场景块索引也能求出场景块的中心点位置。
  2.数据生成规则
  由于还处于开发版本，生成规则简单处理，只对场景中的Prefab进行处理。正式的处理方式，应该是区分远景和近景，加载的时候根据远景和近景来调整加载优先级。
  当前这个这个处理方式为：计算一个Prefab的Bounds，然后根据Bounds四个点。来计算Prefab属于那几个场景块。根据遍历之后的结果，生成相关的数据文件。为了方便调试。同时会生成文本格式的，场景文件。生成的文件夹内容如图2所示。场景Debug文件生成的内容如图3和图4所示。
![@TQ4FU_A00INWUW63NW8A`0](https://github.com/coderhe/SceneLoadInChunck/assets/3722873/c422c25d-e406-4731-afcf-65d5550a2871)
                                        图2 场景生成文件格式
![image](https://github.com/coderhe/SceneLoadInChunck/assets/3722873/97450e07-41b9-4f21-ae52-abf9cf495c60)
                                        图3 场景debug文件debug_asset_id.txt
![image](https://github.com/coderhe/SceneLoadInChunck/assets/3722873/2ff78310-3537-4b99-afb0-2b76783947aa)
                                        图4 场景debug文件debug_scene_info.txt
# 场景分块加载策略
  根据视野范围加载，并且为了能够保持连贯性，不会出现漏场景的的情况出现，所以会在多加载一层，相机属性改变时候，重新计算加载的层数。具体表现如图5所示，其中红框表示屏幕范围，黑框表示加载的场景块范围。
  ![image](https://github.com/coderhe/SceneLoadInChunck/assets/3722873/1bb58b84-ea3a-4ad9-86fb-3b0c2f85e5bc)
                                        图5 场景加载区域
    这个版本还处理的很粗糙，直接按照最大的层数来加载的场景块。正式版本的话应该按照最少加载的原则来实现。
