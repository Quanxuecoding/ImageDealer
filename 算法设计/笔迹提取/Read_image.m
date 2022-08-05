% 用于读取图像，并转化为灰度图像的脚本
% 原始图像保存到 [f,map]，灰度图像保存到 f_gray
% 图像信息保存到info
% 可在运行脚本前，事先定义图像文件的完整路径：Img_filename，则不会打开选择文件的对话框

if ~exist('Img_filename','var')
    % 获取图像文件名，并读取图像到[f,map]
    [filename, pathname] = uigetfile({'*.jpg;*.tif;*.png;*.gif;*.bmp','All Image Files';...
        '*.*','All Files' },'选择一个图像文件');
    if isequal(filename,0)
        disp('User selected Cancel');
        f_gray = [];
        return
    else
        Img_filename = fullfile(pathname, filename);
    end
end

[f,map,alpha_ch] = imread(Img_filename);

info = imfinfo(Img_filename);
%% 判断是否为RGB彩色图像
image_size = size(f);
if numel(image_size) == 3
    f_gray = rgb2gray(f(:,:,1:3));   % 将彩色图像转换为灰度图像
else
    f_gray = f;
end

%% 判断是否为索引图像
if(~isempty(map))        % 如果是索引图像
    f_gray = ind2gray(f,map);
end

% imshow(f_gray);
