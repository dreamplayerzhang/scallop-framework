SensorConfigFile = 'SensorConfig.xml';

global frameCount;
global imgRed;
global imgBG;

frameCount = 0;

%% Load dlls
NET.addAssembly([pwd '\Scallop.Core.dll'])
NET.addAssembly([pwd '\Scallop.Sensor.Axis.dll'])

%% Create Axis camera instance
AxisCamera = Scallop.Core.InterfaceFactory.CreateSensorInstance([pwd '\Scallop.Sensor.Axis.dll'])
dataListener = addlistener(AxisCamera, 'Data', @eventSensorData);
statusListener = addlistener(AxisCamera, 'StatusChanged', @eventSensorStatusChanged);

%% Register sensor
%AxisCamera.Register(System.Xml.Linq.XDocument.Load(SensorConfigFile),'Rotuaari');
AxisCamera.Register(System.Xml.Linq.XDocument.Load(SensorConfigFile),'Linnanmaa');

%% Start sensor
startSensor(AxisCamera);

%% Sensor will stop after 30 seconds
t = timer('TimerFcn', 'stopSensor(AxisCamera)', 'StartDelay', 30.0);
start(t);
