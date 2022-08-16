function Subgraphextraction(inputPath, outputPath)
%用于自动提取子图
%countx与county存储边界线坐标
%CornerPoint细胞数组存储角点坐标
%BoundaryCoordinates细胞数组将角点坐标转换为切割时的横纵坐标(升序排列）

%% 读取图像
% 用于读取图像，并转化为灰度图像的脚本
% 原始图像保存到 [f,map]，灰度图像保存到 f_gray
% 图像信息保存到info
% 可在运行脚本前，事先定义图像文件的完整路径：Img_filename，则不会打开选择文件的对话框
[f, map, ~] = imread(inputPath);

%% 判断是否为RGB彩色图像
image_size = size(f);
if numel(image_size) == 3
    f_gray = rgb2gray(f(:, :, 1:3)); % 将彩色图像转换为灰度图像
else
    f_gray = f;
end

%% 判断是否为索引图像
if (~isempty(map)) % 如果是索引图像
    f_gray = ind2gray(f, map);
end
figure, imshow(f);
title('原图');

%% 边界复制,因为在判断边界时需去掉一次边界（1个像素）
M = image_size(1);
N = image_size(2);
% f_gray = [f_gray(1, :); f_gray; f_gray(M, :)];
% f_gray = [f_gray(:, 1), f_gray, f_gray(:, N)];

%% 判断边界
h1 = [-1, -1, -1; 2, 2, 2; -1, -1, -1];
h2 = [-1, 2, -1; -1, 2, -1; -1, 2, -1];

f_gray = imfilter(f_gray, h1, "replicate") + imfilter(f_gray, h2, "replicate");
figure;
imshow(f_gray);
title('滤波后');

%% 生成蒙版图像 Mask，没有必要
RowSum = sum(f_gray, 2); % 按行求和
ColomnSum = sum(f_gray, 1); % 按列求和
Mask = true(M, N);
Mask(RowSum == 0, :) = false;
Mask(:, ColomnSum == 0) = false;

figure, imshow(Mask);
title('蒙版');

%%
RowSum = RowSum > 0; % RowSum 转变为 逻辑类型
ColomnSum = ColomnSum > 0; % ColomnSum 转变为 逻辑类型

RowSum = [0; RowSum; 0]; % 前后补零，应对 图像原本就没有上下边框的情况
ColomnSum = [0, ColomnSum, 0]; % 前后补零，应对 图像原本就没有左右边框的情况

x = find(diff(RowSum) ~= 0);
y = find(diff(ColomnSum) ~= 0);

x(2:2:end) = x(2:2:end) - 1; % 区域终止行的坐标都加1
y(2:2:end) = y(2:2:end) - 1; % 区域终止列的坐标都加1

%%
count = 0; % 存储子图数
for i = 1:2:length(x)
    for j = 1:2:length(y)
        Subgraph = f(x(i):x(i + 1), ...
            y(j):y(j + 1), :);
        count = count + 1;
                 imwrite(Subgraph, [outputPath,'_',num2str(count) '.bmp']);
        figure, imshow(Subgraph);
    end
end

end
