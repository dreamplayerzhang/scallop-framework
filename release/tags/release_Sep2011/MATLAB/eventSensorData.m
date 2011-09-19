function eventSensorData(sender, ScallopSensorDataEventArgs)
   
   global frameCount;
   global imgBG;
   global imgRed;
   global hFig;
   global hIm;
   global hRed;
   
   %% Get the pointer to pixel data
   bmp = ScallopSensorDataEventArgs.Data;
   width = bmp.Width;
   height = bmp.Height;
   rect = System.Drawing.Rectangle(0,0,width,height);
   bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

   ptr = bmpData.Scan0;

   %% Declare an array to hold the bytes of the bitmap.
   % This code is specific to a bitmap with 24 bits per pixels.
   bytes = bmp.Width * bmp.Height * 3;
   rgbValues = NET.createArray('System.Byte', bytes);
   
   %% Copy the RGB values into the array.
   System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

   img = uint8(rgbValues);
   
   b=img(1:3:end);
   g=img(2:3:end);
   r=img(3:3:end);
   
   b=reshape(b,width,height)';
   g=reshape(g,width,height)';
   r=reshape(r,width,height)';
   
   %% Combibe the color channels
   img = cat(3,r,g,b);
   
   if(frameCount==0)
      hFig = figure('Name','Scallop MATLAB Demo','MenuBar','none','NumberTitle','off');
      hIm = imshow(zeros(height,width,'uint8'),'Border','Tight');
      
      imgBG = rgb2gray(img);
      imgRed = cat(3,ones(height,width), zeros(height,width), zeros(height,width));
      
      hold on;
      hRed = imshow(imgRed,'Border','Tight');
      hold off;
      
      frameCount = 1;
      return;
   end
   
   %% Do image processing...
   imgDiff = abs(rgb2gray(img)-imgBG);
   imgDiff(imgDiff>8)=255;
   
   %% Show image
   set(hIm,'CData',img);
   set(hRed, 'AlphaData', imgDiff);
   
   drawnow;
   
   imgBG = uint8( 0.8*rgb2gray(img) + 0.2*imgBG );
   frameCount = frameCount + 1;
   %disp(['Frame ' num2str(frameCount) '  Size: ' num2str(width) 'x' num2str(height)]);

end