   M S B A M L   `   `   `  ����  
Cost Split    Cost_Split.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Maple Cost Splits��5   	    � Left$ 0��6#   $	��700��6,   $
��1200��69    � Icon$ 
./cost.ico��5   	   .��5      �  � RowDefinitions 5   
   �� 5      $��40 �6   6*   �� 5	      $��* �6   6)   .+�5   
   � $��0��6   $	��Top=�6!   $��40��69   $��0,0,0,0q�6E   .+�5      �� $��	From Date��6   $
��Left=�6(   $��	10,10,0,0q�6C   $	��Top=�6V    J� RenderTransformOrigin$ -1.346,-1.134&�6n   (  "System.Windows.Controls.DatePicker  5      -   5          Name$ DateFromPicker��5      $
��Left=�61   $��	10,10,0,0q�6M   $	��Top=�6`   �� 5      $��To Date��6   $
��Left=�6&   $��	10,10,0,0q�6A   $ -1.346,-1.134&�6T   $	��Top=�6z   $��51��6�     5      -   5        $ DateToPicker��5      $
��Left=�6/   $��	10,10,0,0q�6J   $	��Top=�6]   $	��112��6u   �� 5      $��Employee��6   $
��Left=�6'   $��	10,10,0,0q�6B   $ -1.346,-1.134&�6U   $	��Top=�6{   $��62��6�   $��1��6�   �� 5      -   5         ��Name$ EmployeeTextbox��5      $
��Left=�6/   $��	10,10,0,0q�6J    �� TextWrapping$
 Wrap=�6]   $��TextBox��6q   $	��Top=�6�   $��1��6�   $��99��6�   �� 5      -   5         ��Name$ QueryButton��5      $��Query��6*   $
��Left=�6:   $��	10,10,0,0q�6U   $	��Top=�6h   $��59��6�   	 � ToolTip$J	 DQuery data already retrieved from Jira. Hit Fetch to get latest data��6�   �� 5      -   5        $ 
CopyButton��5      $��	Copy Grid��6)   $
��Left=�6=   $��	10,10,0,0q�6X   $	��Top=�6k   $��75��6�   $y�Hidden=�6�   �� 5      -   5        $ FetchButton��5      $��Fetch��6*   $
��Left=�6:   $��	10,10,0,0q�6U   $	��Top=�6h   $��59��6�   $!	 Fetch latest data from Jira��6�     5      -   5        $ FetchFromDatePicker��5      $
��Left=�66   $��	10,10,0,0q�6Q   $	��Top=�6d   $	��112��6|   5      &   System.Windows.Controls.DataGrid  5   
   -	   5        
  Name$
 
ResultGrid��5       $��1��6      
IsReadOnly .    AlternatingRowBackground
 ��ح� �� AlternationCount$ 2��6k   ��. $
��Auto=�5      $
��Auto=�5      5      5      