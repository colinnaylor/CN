   M S B A M L   `   `   `  ���� H  DSwiftImporter, Version=1.1.2.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:SwiftImporter.HelperControlsSwiftImporter.HelperControls      SwiftImporter.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     9uc*clr-namespace:SwiftImporter.HelperControls       � Icon$  import-export.ico�� � Title$ Swift Importer UI��$
��Auto��$
��1000��.���� .+�o� .@�m� $
��File��.@�m� -    m�Name$ exitMenu��$
��Exit��m� $
��Help��.@�m� -   $ 	aboutMenu��$��About���� $��Status��$��5,5,5,5q�.���� .+��� $	��100��$
��Auto=�$
��Auto=�.���� -    ��Name$ statusTxtBox�� �� 
IsReadOnly . ����  �� Path$ Status�]� .+��� ���� $ ProgressPercentage�� -    �Name$ progBar�� � Minimum$ 0Q� � Maximum$	 100Q�$��5,5,5,5q��� $��Show status updates��	 d� 	IsChecked	 �� $ KeepStatusInMemory�
 �� Mode$
 TwoWay=��� $��Auto Import��$��5,5,5,5q�.���� .+���  �� Orientation$ 
Horizontal=�$��5,5,5,5q�.+��� $��Location���� -   $ loactionTxtBox��$	��500������ $ MonitoringPath� �� UpdateSourceTrigger$ PropertyChanged=� �� ValidatesOnExceptions . �� $ 
Horizontal=�.+�/   )SwiftImporter.HelperControls.IPhoneSlider  -     Name$ monitorSlider��   Label$ Monitor��$
��Left=�$��5,5,5,5q� � ToolTip$T NMonitor the specified file location for new files to be automatically imported��   IsOn �� $ IsMonitoring�$
 TwoWay=� �� IsAsync .   -   $ timerSlider��$ Timer��$
��Left=�$��5,5,5,5q�$M GDaily timer to schedule an automatic import from the specified location�� �� $ IsTimerScheduled�$
 TwoWay=� . �� $��Manual Import��$��5,5,5,5q�.���� .+�]� ��. .+��� $ 
Horizontal=�.+��� $��5,5,5,5q�$	��500������ $ FilePath�$
 TwoWay=�$ PropertyChanged=��� $��Import File��$��5,5,5,5q����� $ ImportFileCommand��� $��Import All Files (*.out)��$��5,5,5,5q����� $ ImportAllFilesCommand��� $��Cancel��$��5,5,5,5q�$��Right=�$��Right=����� $ CancelCommand��� .��&   System.Windows.Controls.DataGrid  -     Name$ StatementGrid��$	��200������ $ 
Statements��� .��  $	��200������  �� ElementName$ StatementGrid��$&  SelectedItem.SwiftStatementItems��� $��Save Swift Data��$��Right=�$��5,5,5,5q����� $ SaveStatementsCommand�