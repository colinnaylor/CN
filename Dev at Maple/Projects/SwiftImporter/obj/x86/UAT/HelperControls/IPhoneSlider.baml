   M S B A M L   `   `   `  ����   SwiftImporter/    )SwiftImporter.HelperControls.IPhoneSlider   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008       � 	Resources     CheckBoxStyle1 �� 
TargetType �� Property �� Value �� 
TargetType � 	Resources  Key g� 	BeginTime �� 
TargetName �� TargetProperty	 O� KeyTime
 O� Value ]�Name J� SnapsToDevicePixels �� ContentStringFormat �� ContentStringFormat �� CornerRadius �� 
StartPoint �� EndPoint � Color � Offset ��Name J� RenderTransformOrigin �� ScaleX �� ScaleY �� AngleX �� AngleY �� Angle T� X T� Y �� Triggers P� Property P� Value  �� Triggers! N� ExitActions" ��Name  OnUnchecking# N� EnterActions  
OnChecking��%�  &	        7  7  �� " ��.���  � ��# � ����  � ��# � ����  � �� �� " �� ���� 
OnChecking .��U� $ 00:00:00X�$ slider��$U O(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)�.\��� $	 00:00:00.2000000��$
 29Q��� OnUnchecking .��U� $ 00:00:00X�$ slider��$U O(UIElement.RenderTransform).(TransformGroup.Children)[3].(TranslateTransform.X)�.\��� $	 00:00:00.2000000��$
 0Q�.R�]� $ 	dockPanel��.+��� # z #��z��# z #��z����. $��Center=��� $��5,0,0,0q�$��60��$��26��$��1q�$��Right=�$ 4������ " ��.���  � ��
 �����._�� $��60��.+��� $��ON��$
��Bold"�
�������$��35��$
��Left=�$
��Left=����� .���  � �� �� $	 0,0&�$	 0,1&�.��� $ 	#FF4B96FA��$ 0.996Q�� $ 	#FF7DC8FF��$ 0.705Q�� $ 	#FF001EE1��$ 0Q�� $ 	#FF327DFA��$ 0.348Q��� $	��OFF��$��Right=�$��30��$��Center=����� .���  � �� �� $ 0.5,1&�$ 0.5,0&�.��� $ 	#FFF0F0F0��$ 0.996Q�� $ 	#FFFFFFFF��$ 0.705Q�� $ 	#FFE1E1E1��$ 0Q�� $ 	#FFF0F0F0��$ 0.353Q��� $ slider��$
��31.5��$
��Left=�$ 0.5,0.5&�$��1q�$ 3��z�V� .~��� $ 1Q�$ 1Q��� $ 0Q�$ 0Q��� $ 0Q�T� $ 0Q�$ 0Q����� " ��.���  � �� �� $ 0.5,1&�$ 0.5,0&�.��� $ 	#FFBEBEBE��$ 0.996Q�� $ 	#FFFAFAFA��$ 0.705Q�� $ 	#FF969696��$ 0Q�� $ 	#FFBEBEBE��$ 0.348Q���  � ��
 ����� P�  �@J�IsMouseOver . . ���  � ��
 �   �  P�  �@d�	IsChecked$
 TrueY�! �� $"" OnUnchecking_BeginStoryboard��8��  # �� $ " OnChecking_BeginStoryboard��8�� P�  � {� .  . ��� # � �� � ��.��� -   $ �Name$$ 
LayoutRoot��% � DataContext% �� & �� RelativeSource& �� *FindAncestor+' �� AncestorType"' >�.+��� -   ( ��Name$( 
mainChkBox��$
��Left=�#���   $��Center=�$	��TBD��) d� 	IsChecked) �� * �� Path$
* IsOn�+ �� Mode$+ TwoWay=�