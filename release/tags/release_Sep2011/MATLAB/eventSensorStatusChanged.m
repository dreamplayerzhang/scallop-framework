function eventSensorStatusChanged(sender, eventArgs)
   newState = eventArgs.NewState;
   disp('Sensor status changed to: ');
   disp(newState);
end