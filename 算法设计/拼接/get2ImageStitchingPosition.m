function [dx, dy, success] = get2ImageStitchingPosition(input_A, input_B)
% GETIMAGESTITCHINGPOSITION 此处显示有关此函数的摘要
%   此处显示详细说明
success = false;
dx = 0;
dy = 0;

image_A = imread(input_A);
image_B = imread(input_B);

%% CONVERT TO GRAY SCALE
gray_A   = im2double(rgb2gray(image_A));
gray_B   = im2double(rgb2gray(image_B));
[M1, N1] = size(gray_A);
[M2, N2] = size(gray_B);

%% FIND HARRIS CORNERS IN BOTH IMAGE
[x_A, y_A, v_A] = harris(gray_A, 2, 0.0, 2);
[x_B, y_B, v_B] = harris(gray_B, 2, 0.0, 2);

%% ADAPTIVE NON-MAXIMAL SUPPRESSION (ANMS)
ncorners = 500;
[x_A, y_A, ~] = ada_nonmax_suppression(x_A, y_A, v_A, ncorners);
[x_B, y_B, ~] = ada_nonmax_suppression(x_B, y_B, v_B, ncorners);

%% 显示所有找到的角点
% figure(1);
% hold off
% imshow(gray_A)
% hold on
% plot(y_A, x_A, 'r*');
% 
% figure(2);
% hold off
% imshow(gray_B)
% hold on
% plot(y_B, x_B, 'r*');

%%
% EXTRACT FEATURE DESCRIPTORS
sigma   = 7;
[des_A] = getFeatureDescriptor(gray_A, x_A, y_A, sigma);
[des_B] = getFeatureDescriptor(gray_B, x_B, y_B, sigma);

% IMPLEMENT FEATURE MATCHING
dist = dist2(des_A, des_B); % 方阵，描述子 A、B 之间的距离
[ord_dist, index] = sort(dist, 2);

%% 判断哪些角点，特征向量完全相同，即距离为零。
% BW_dist0 = (dist == 0);
[row, col] = find(dist == 0); % 距离方阵 dist 中的坐标

%% 显示特征向量完全相同的角点
% figure(1);
% hold off
% imshow(gray_A)
% hold on
% plot(y_A(row), x_A(row), 'r*');
% plot(y_A(row(1)), x_A(row(1)), 'go', 'LineWidth', 3);
% 
% figure(2);
% hold off
% imshow(gray_B)
% hold on
% plot(y_B(col), x_B(col), 'r*');
% plot(y_B(col(1)), x_B(col(1)), 'go', 'LineWidth', 3);

%% 遍历所有特征点，判断重叠区域是否相同
success = false;
for ii = 1:length(col)

    %% 假设当前特征点是两幅图像共有的点，计算特征点到重叠区域的边距
    left   = min(y_A(row(ii)), y_B(col(ii)));
    top    = min(x_A(row(ii)), x_B(col(ii)));
    right  = min(N1-y_A(row(ii)), N2-y_B(col(ii)));
    bottom = min(M1-x_A(row(ii)), M2-x_B(col(ii)));

    %% 提取出两幅输入图像中，重叠的区域
    overlap_A = gray_A(x_A(row(ii))-top+1:x_A(row(ii))+bottom, y_A(row(ii))-left+1:y_A(row(ii))+right);
    overlap_B = gray_B(x_B(col(ii))-top+1:x_B(col(ii))+bottom, y_B(col(ii))-left+1:y_B(col(ii))+right);

    %     figure('Name', 'Pic A 重叠部分');
    %     imshow(overlap_A);
    %     figure('Name', 'Pic B 重叠部分');
    %     imshow(overlap_B);

    if isequal(overlap_A, overlap_B)

%         %% 计算结果图像的大小
%         left   = max(y_A(row(ii)), y_B(col(ii)));
%         top    = max(x_A(row(ii)), x_B(col(ii)));
%         right  = max(N1-y_A(row(ii)), N2-y_B(col(ii)));
%         bottom = max(M2-x_A(row(ii)), M2-x_B(col(ii)));
% 
%         M3 = top + bottom; % 拼接结果图像的行数
%         N3 = left + right; % 拼接结果图像的列数
% 
%         %% 将两个输入图像，拷贝到结果图中
%         gray_C = ones(M3, N3, 3);
%         gray_C(top-x_A(row(ii))+1:top-x_A(row(ii))+M1, left-y_A(row(ii))+1:left-y_A(row(ii))+N1, 1:3) = im2double(image_A);
%         gray_C(top-x_B(col(ii))+1:top-x_B(col(ii))+M2, left-y_B(col(ii))+1:left-y_B(col(ii))+N2, 1:3) = im2double(image_B);
%         figure('Name', '拼接结果');
%         imshow(gray_C);
        success = true;

        %% 原点坐标差值，正值表示图B，相对于图A 偏右，偏下
        dx = x_A(row(ii)) -  x_B(col(ii));
        dy = y_A(row(ii)) - y_B(col(ii));

        % 重叠点坐标
        % [x_A(row(1)), y_A(row(1))]
        % [x_B(col(1)), y_B(col(1))]

        break;
    end
end

end
