   M S B A M L   `   `   `  ���� F  BNeruTrainer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:NeruTrainerNeruTrainer      NeruTrainer.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     +localclr-namespace:NeruTrainer       � Title$  神经网络训练机��$	��542��$
��1104��.���  � ColumnDefinitions �� $
��283* ��� $
��265* �.+��� -    ��Name$ 
tbReadPath��$
��Left=�$��23��$��
108,66,0,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��120��$��1���� -    ��Name$ btReadTrain��$��读取训练数据��$
��Left=�$��1,67,0,0q�$	��Top=�$��84��$��22�����	#FFE4E4E4���	#FF190707$��1���� -   $ tbSavefileName��$
��Left=�$��23��$��108,107,0,0q�$
 Wrap=�$	��Top=�$	��120��$��1���� -   $ btSaveTrain��$��保存训练结果��$
��Left=�$��	1,110,0,0q�$	��Top=�$��84��$��22��$��1��{�.  �� -   $ tbInformation��$	��456��$��
41,19,35,0q�$
 Wrap=�$	��Top=��� -    ��Name$ label��$��输入层节点数��$
��Left=�$��
260,17,0,0q�$	��Top=�$��1��$��25��$��82���� -   $ label1��$��隐含层节点数��$
��Left=�$��
260,48,0,0q�$	��Top=�$��1��$��25��$��82���� -   $ label2��$��输出层节点数��$
��Left=�$��
260,79,0,0q�$	��Top=�$��1��$��25��$��82���� -	   $ tbInputNodeNum��$
��Left=�$��23��$��
347,17,0,0q�$
 Wrap=�$	��Top=�$	��120�� �� SelectionOpacity$ 8Q�$��4��$��1���� -
   $ tbHideNodeNum��$
��Left=�$��23��$��
347,48,0,0q�$
 Wrap=�$	��Top=�$	��120��$��8��$��1���� -   $ tbOutputNodeNum��$
��Left=�$��23��$��
347,80,0,0q�$
 Wrap=�$	��Top=�$	��120��$��3��$��1���� -   $ btStartTrain��$��开始训练��$
��Left=�$��382,173,0,0q�$	��Top=�$��85��$��45��$��1���� -    ��Name$ cbTrigFunction��$
��Left=�$��347,117,0,0q�$	��Top=�$	��120��$��1��$��21���� -   $ label4��$��激活函数��$
��Left=�$��283,117,0,0q�$	��Top=�$��1��$��25��$��58���� -   $ label5��$��隐藏层权重速率��$
��Left=�$��	1,148,0,0q�$	��Top=�$��1��$��25��$��94���� -   $ 
tbRateHide��$
��Left=�$��23��$��108,150,0,0q�$
 Wrap=�$	��Top=�$	��120��$��1��$	��0.1���� -   $ 	tbRateOut��$
��Left=�$��23��$��108,191,0,0q�$
 Wrap=�$	��Top=�$	��120��$��1��$	��0.1���� -   $ label6��$��输出层权重速率��$
��Left=�$��	1,189,0,0q�$	��Top=�$��1��$��25��$��94���� -   $ tbReTime��$��1��$
��Left=�$��23��$��108,233,0,0q�$
 Wrap=�$��1��$	��Top=�$	��120���� -   $ label7��$��迭代次数��$��1��$
��Left=�$��
27,231,0,0q�$	��Top=� J� RenderTransformOrigin$ -0.225,0.32&�$��25��$��58���� -   $ btReadTrainW��$��读取训练结果��$��1��$
��Left=�$��1,19,0,0q�$	��Top=�$��84��$��23���� -   $ textBox��$��1��$
��Left=�$��23��$��
108,19,0,0q�$
 Wrap=�$	��Top=�$	��120���� -   $ 
btDisplayW��$��当前权值显示��$��1��$
��Left=�$��283,173,0,0q�$	��Top=�$��86��$��45��{�.  �� -   $ btTest��$
��Test��$��1��$
��Left=�$��382,231,0,0q�$	��Top=�$��85���� -   $ btDisnetwork��$��图示网络结构��$��1��$
��Left=�$��283,231,0,0q�$	��Top=�$��85���� -   $ button��$��神经网络验证机��$��1��$
��Left=�$��283,266,0,0q�$	��Top=�$	��184��$��56��