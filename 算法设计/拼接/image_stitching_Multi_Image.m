ccc;
dir_Output = dir(fullfile('.\测试图像\', '待拼接原图*.png'));
fileNames_Source = {dir_Output.name}';
pathNames_Source = {dir_Output.folder}';
nFiles = length(dir_Output);

%% 初始化关系矩阵
flagMatrix = false(nFiles); % 重叠标志
dxMatrix   = zeros(nFiles); % 原点偏移量：x 方向
dyMatrix   = zeros(nFiles); % 原点偏移量：y 方向

%% 判断各子图间的重叠关系，若有重叠，则记录 flag 在 flagMatrix 中
for ii = 1:nFiles - 1
    for jj = ii + 1:nFiles
        disp([num2str(ii), '   ', num2str(jj)]);
        input_A = fullfile(pathNames_Source{ii}, fileNames_Source{ii});
        input_B = fullfile(pathNames_Source{jj}, fileNames_Source{jj});

        [dx, dy, success]  = get2ImageStitchingPosition(input_A, input_B);
        flagMatrix(ii, jj) = success;
        dxMatrix(ii, jj)   = dx;
        dyMatrix(ii, jj)   = dy;
    end
end

%% 构建完整的关系矩阵
% flag0 = flagMatrix;
% flagMatrix = flagMatrix + flagMatrix';
% dxMatrix   = dxMatrix - dxMatrix';
% dyMatrix   = dyMatrix - dyMatrix';

%% 构建以 1 为 Root 的树
clc
recordedValue = 1;

t = tree(1);
for ii = 1:nFiles
    row = flagMatrix(ii, :); % 取出当前行
    row = find(row > 0);     % 找到当前行中的非 0 值
    for jj = 1:length(row)
        if ~ismember(row(jj), recordedValue)
            index_of_jj = find(t == ii);
            t = t.addnode(index_of_jj, row(jj));
            recordedValue = [recordedValue, row(jj)];
        end
    end
    if length(recordedValue) == nFiles
        break;
    end
end
disp(t.tostring);

%% 计算匹配到图1的坐标转换路径：path，以及最终的转换坐标：dx_Relativeto1，dy_Relativeto1
dx_Relativeto1 = dxMatrix(1, :);
dy_Relativeto1 = dyMatrix(1, :);

for ii = 2:nFiles
    path = t.findpath(1, ii);
    if length(path) > 2
        for jj = length(path):-1:2
            dx_Relativeto1(t.get(ii)) = dx_Relativeto1(t.get(ii)) + dxMatrix(t.get(path(jj - 1)), t.get(path(jj)));
            dy_Relativeto1(t.get(ii)) = dy_Relativeto1(t.get(ii)) + dyMatrix(t.get(path(jj - 1)), t.get(path(jj)));
        end
    end
end

%% 获取图像原始大小
MM = zeros(1, nFiles); % 行数
NN = zeros(1, nFiles); % 列数
for ii = 1:nFiles
    info   = imfinfo(fullfile(pathNames_Source{ii}, fileNames_Source{ii}));
    MM(ii) = info.Height;
    NN(ii) = info.Width;
end

%% 计算最终画布大小：MN_final = [M_final, N_final]，即行列数
% 左上角
UpperLeftConner = [min(dx_Relativeto1), min(dy_Relativeto1)];
% 右下角
LowerRightCorner = [max(dx_Relativeto1 + MM), max(dy_Relativeto1 + NN)];

MN_final = LowerRightCorner - UpperLeftConner;

%% 计算最终的转换坐标
% 即：图 1 不一定是左上角的图像时，需要将偏移量，都调整为相对于最终的输出图像左上角
dxFinal = dx_Relativeto1 - UpperLeftConner(1);
dyFinal = dy_Relativeto1 - UpperLeftConner(2);

%% 合并所有图像到输出结果矩阵：f_output
nChannel = info.BitDepth / 8;
f_output = uint8(zeros([MN_final, nChannel]));

for ii = 1:nFiles
    f = imread(fullfile(pathNames_Source{ii}, fileNames_Source{ii}));
    f_gray = rgb2gray(f);
    f_output(dxFinal(ii)+1:dxFinal(ii)+MM(ii), dyFinal(ii)+1:dyFinal(ii)+NN(ii), :) = f;
end
imshow(f_output)