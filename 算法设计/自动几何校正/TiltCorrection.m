function img = TiltCorrection(inputPath)
%用于图像倾斜校正
%% 读取图像
% 用于读取图像，并转化为灰度图像的脚本
% 原始图像保存到 [f,map]，灰度图像保存到 f_gray
% 图像信息保存到info
% 可在运行脚本前，事先定义图像文件的完整路径：Img_filename，则不会打开选择文件的对话框
[f, map, ~] = imread(inputPath);

%% 判断是否为RGB彩色图像
image_size = size(f);
if numel(image_size) == 3
    f_gray = rgb2gray(f(:, :, 1: 3));%彩色图像转换为灰度图像
else
    f_gray = f;
end

%% 判断是否为索引图像
if (~isempty(map)) %是索引图像
    f_gray = ind2gray(f, map);
end
figure, imshow(f); title('原图');

%% 基于最小误差的阈值分割
[counts, gray_style] = imhist(f_gray);
gray_level = length(gray_style); %亮度级别
gray_probability  = counts ./ sum(counts); %计算各灰度概率
gray_mean = gray_style' * gray_probability; %统计像素平均值
gray_vector = zeros(gray_level, 1);
w = gray_probability(1);
mean_k = 0;
gray_vector(1) = realmax;
ks = gray_level - 1;
for k = 1: ks
    % 迭代计算
    w = w + gray_probability(k + 1);
    mean_k = mean_k + k * gray_probability(k + 1);
    % 判断是否收敛
    if (w < eps) || (w > 1-eps)
        gray_vector(k + 1) = realmax;
    else
        % 计算均值
        mean_k1 = mean_k / w;
        mean_k2 = (gray_mean - mean_k) / (1 - w);
        % 计算方差
        var_k1 = (((0: k)' - mean_k1) .^ 2)' * gray_probability(1: k + 1);
        var_k1 = var_k1 / w;
        var_k2 = (((k + 1: ks)' - mean_k2) .^ 2)' * gray_probability(k + 2: ks + 1);
        var_k2 = var_k2 / (1 - w);
        % 计算目标函数
        if var_k1 > eps && var_k2 > eps
            gray_vector(k + 1) = 1 + w * log(var_k1) + (1 - w) * log(var_k2) - 2 * w * log(w)...
                - 2 * (1 - w) * log(1 - w);
        else
            gray_vector(k + 1) = realmax;
        end
    end
end
min_gray_index = find(gray_vector == min(gray_vector)); %极值统计
min_gray_index = mean(min_gray_index);
threshold_kittler = (min_gray_index - 1) / ks; %计算阈值
f_bin = imbinarize(f_gray, threshold_kittler);% 阈值分割
%figure, imshow(f_bin); title('二值化图像');

%% 寻找最小外接矩形
f_bin = bwmorph(f_bin, 'close');
f_bin = bwmorph(f_bin, 'open');

f_bin = ~f_bin;

f_bin(1, :) = 0;
f_bin(:, 1) = 0;
f_bin(image_size(1), :) = 0;
f_bin(:, image_size(2)) = 0;
f_bin = wiener2(f_bin, [5, 5]); %二维维纳滤波函数去除离散噪声点

[r c] = find(f_bin == 1);
% 'a'是按面积算的最小矩形，如果按边长用'p'
[rectx, recty, area, perimeter] = minboundrect(c, r, 'p'); %四个角点坐标

figure, imshow(f_bin);title('最小外接矩形提取');
line(rectx,recty);

%% 旋转图像
sidelength1 = sqrt((rectx(1) - rectx(2)) ^ 2 + (recty(1) - recty(2)) ^ 2);
sidelength2 = sqrt((rectx(2) - rectx(3)) ^ 2 + (recty(2) - recty(3)) ^ 2);
if sidelength1 >= sidelength2 %求最长边斜率
    slope = (recty(1) - recty(2)) / (rectx(1) - rectx(2));
else
    slope = (recty(1) - recty(4)) / (rectx(1) - rectx(4));
end
angle = atan(slope) * 180 / pi;
f = imcomplement(f);
f_cor = imrotate(f, angle, 'bilinear', 'loose'); %旋转图像，使用双线性插值
f_cor = imcomplement(f_cor);
img = f_cor;
figure, imshow(img);title('旋转后');

end











