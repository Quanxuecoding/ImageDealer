%% 读入图像文件
close all
Read_image

figure
imshow(f);
title('原始图像')

%% 图像剪裁，截取出需要的部分
f_cropped = imcrop(f);
if isempty(f_cropped)
    f_cropped = f;
end
f_gray = im2gray(f_cropped);

%% Bottom Hat 变换，去除阴影
se = strel('disk', 10);
g = imbothat(f_gray, se);

%% Otsu 方法 图像分割

BW = imbinarize(g);

%% 平滑笔划边界
SE1 = strel('diamond', 1);
BW = imopen(BW, SE1);
SE2 = strel('square', 3);
BW = imopen(BW, SE2);

%% 以BW为模板，提取原始灰度图像
image_size = size(f_cropped);

Result_pic = f_cropped;
if numel(image_size) == 3
    R = Result_pic(:, :, 1);
    G = Result_pic(:, :, 2);
    B = Result_pic(:, :, 3);
    R(~BW) = 255;
    G(~BW) = 255;
    B(~BW) = 255;
    Result_pic(:, :, 1) = R;
    Result_pic(:, :, 2) = G;
    Result_pic(:, :, 3) = B;
else
    f_cropped(~BW) = 255;
end

%% 灰度/RGB通道，数值范围自动拉伸到 0~255
MaxI = max(Result_pic(:));
MinI = min(Result_pic(:));
Result_pic = uint8(double(Result_pic - MinI)./double(MaxI)*255);

%% 测量笔画宽度
MAXWIDTH = 50; % 假设最大笔划宽度为50*2像素，再大就不考虑了
delta_S_record = zeros(MAXWIDTH, 1);
S = sum(BW(:));
for ii = 2:100
    se = strel('square', ii);
    BW_eroded = imerode(BW, se);
    S1 = sum(BW_eroded(:));
    delta_S = S - S1;
    S = S1;
    delta_S_record(ii) = delta_S;
    if delta_S == 0
        break;
    end
end
figure
plot(delta_S_record, '-*');
[fwhm, fwtm] = FWHM(delta_S_record);
LineWidth = round((fwhm(2)-fwhm(1))*2);
%     figure('Name', ['BW_eroded' num2str(ii)]);
%     %     imshow(BW_eroded);
%     imshowpair(BW,BW_eroded);

%% 确定文字区域
row_Sum = sum(~BW, 2);
colomn_Sum = sum(~BW, 1);
x = find(row_Sum ~= row_Sum(1));
y = find(colomn_Sum ~= colomn_Sum(1));

%% 截取出文字区域
res_cropped = imcrop(Result_pic, [y(1), x(1), y(end) - y(1), x(end) - x(1)]);
BW_cropped = imcrop(BW, [y(1), x(1), y(end) - y(1), x(end) - x(1)]);

size_cropped = size(res_cropped);
size_res = size_cropped;

%% 添加白色边框
margin_Width = 20;      % 预设的边框宽度
size_res(1) = size_res(1) + margin_Width * 2;
size_res(2) = size_res(2) + margin_Width * 2;
Result_pic2 = uint8(zeros(size_res)) + 255;
Result_pic2(margin_Width+1:margin_Width+size_cropped(1), margin_Width+1:margin_Width+size_cropped(2), :) = res_cropped;
Result_BW = false(size_res(1), size_res(2));
Result_BW(margin_Width+1:margin_Width+size_cropped(1), margin_Width+1:margin_Width+size_cropped(2), :) = BW_cropped;

%%
figure('Name', '剪裁后的结果图像');
imshow(Result_pic2);
figure('Name', 'BW');
imshow(Result_BW);

%% 保存结果到透明背景的png格式文件

alpha = double(Result_BW);
[filename, pathname] = uiputfile({'*.png'}, '保存到', 'Signature.png');
imwrite(Result_pic2, fullfile(pathname, filename), "png", 'Alpha', alpha);

%%
TileWindows