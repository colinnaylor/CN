   M S B A M L   `   `   `  ����H  DSwiftImporter, Version=1.1.2.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:SwiftImporter.HelperControlsSwiftImporter.HelperControls  5   	       SwiftImporter.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   9uc*clr-namespace:SwiftImporter.HelperControls     5   	     � Icon$  import-export.ico��5   	    � Title$ Swift Importer UI��5   	   $
��Auto��6#   $
��1000��61   .��5      �� .+�5	   
   o� .@�5
      m� $
��File��6   .@�5      m� -   5         m�Name$ exitMenu��5      $
��Exit��6-   6;   5      m� 5      $
��Help��6   .@�5      m� -   5        $ 	aboutMenu��5      $��About��6.   6=   5      5      �� 5   
   $��Status��6   $��5,5,5,5q�6#   .��5      �� .+�5      �� $	��100��6   $
��Auto=�6,   $
��Auto=�6O   .��5      �� -   5         ��Name$ statusTxtBox��5       �� 
IsReadOnly . ��64   ��  �� Path$ Status�6Q   5      ]� 5      .+�5      �� ��6   �� $ ProgressPercentage�� 5      -   5         �Name$ progBar��5   "    � Minimum$ 0Q�63    � Maximum$	 100Q�6?   $��5,5,5,5q�6M   5      �� 5      $��Show status updates��6   	 d� 	IsChecked	 69   �� $ KeepStatusInMemory�
 �� Mode$
 TwoWay=�5      5      �� 5   
   $��Auto Import��6   $��5,5,5,5q�6(   .��5      �� .+�5       ��  �� Orientation$ 
Horizontal=�6   $��5,5,5,5q�66   .+�5!      �� $��Location��6   �� 5"      -   5        $ loactionTxtBox��5"      $	��500��6�   ��66   �� $ MonitoringPath� �� UpdateSourceTrigger$ PropertyChanged=� �� ValidatesOnExceptions . 6�   5#      �� 5$      $ 
Horizontal=�6   .+�5%      /   )SwiftImporter.HelperControls.IPhoneSlider  -   5          Name$ monitorSlider��5%   &      Label$ Monitor��5&   &   $
��Left=�66   $��5,5,5,5q�6Q    � ToolTip$T NMonitor the specified file location for new files to be automatically imported��5'   &      IsOn 5%   =   �� $ IsMonitoring�$
 TwoWay=� �� IsAsync . 5'   &     5(      -   5        $ timerSlider��5(   &   $ Timer��5)   &   $
��Left=�64   $��5,5,5,5q�6O   $M GDaily timer to schedule an automatic import from the specified location��5*   &    5(   ;   �� $ IsTimerScheduled�$
 TwoWay=� . 5*   &   5+      5,      5-      �� 5.   
   $��Manual Import��6   $��5,5,5,5q�6*   .��5/      �� .+�50      ]� ��. .+�51      �� $ 
Horizontal=�6!   .+�52      �� $��5,5,5,5q�6s   $	��500��6�   ��6"   �� $ FilePath�$
 TwoWay=�$ PropertyChanged=�6�   �� 53      $��Import File��6!   $��5,5,5,5q�6b   ��67   �� $ ImportFileCommand�6b   �� 54      $��Import All Files (*.out)��6!   $��5,5,5,5q�6s   ��6D   �� $ ImportAllFilesCommand�6s   55      �� 56      $��Cancel��6   $��5,5,5,5q�6U   $��Right=�6g   $��Right=�6�   ��6.   �� $ CancelCommand�6�   57      �� 58      .��59      &   System.Windows.Controls.DataGrid  -   5          Name$ StatementGrid��59      $	��200��6^   ��66   �� $ 
Statements�6^   5:      �� 5;      .��5<        $	��200��6x   ��6   ��  �� ElementName$ StatementGrid��$&  SelectedItem.SwiftStatementItems�6x   5=      �� 5>      $��Save Swift Data��6   $��Right=�6b   $��5,5,5,5q�6~   ��63   �� $ SaveStatementsCommand�6~   5?      5@      5B      5C      