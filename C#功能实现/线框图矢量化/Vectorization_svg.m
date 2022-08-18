function Vectorization_svg(inputPath)
%Read_image();%读取图像
filename_svg = 'res.svg';%结果保存
%inputPath = 'v2.png';

%读取图像
[f, map, ~] = imread(inputPath);
image_size = size(f);

%二维图转化为三维
if length(image_size) == 2
    temp(:, :, 1) = f;
    temp(:, :, 2) = f;
    temp(:, :, 3) = f;
    f = temp;
end

%统计颜色数量
Color = uint32(f(:, :, 1));
Color = 256 * Color + uint32(f(:, :, 2));
Color = 256 * Color + uint32(f(:, :, 3));
Color = nnz(diff(sort(Color(:)))) + 1;

%输出的颜色数量
Colornum = 27;
figure, imshow(f); title('原图');

%中值滤波，如果图像简单，则不进行
if Color > 10000
    f_Origin = img_medfilt(f, 5);
else
     f_Origin = f;
end
figure, imshow(f_Origin); title('中值滤波后的图像');
%f_Origin = f;

%颜色量化
TotalPixels = image_size(1) * image_size(2);
[X, camp] = rgb2ind(f_Origin, Colornum, 'nodither');

%去除孤立像素
X = Del_IsolatedPixel(X);
%X = Del_IsolatedPixel(X);
figure, imshow(ind2rgb(X, camp)); title('去除孤立像素后');

%对每一种颜色进行分割，保存轮廓与颜色信息
temp = 0;
for k = 1: size(camp, 1)
    BW_k = (X == (k - 1));%索引对应的分别为0到(N-1)，所以要减1
    C_BW_K = bwconncomp(BW_k);%进行区域分割识别
    Pix_Area = C_BW_K.PixelIdxList;
    N_Area = numel(Pix_Area);
    for m = 1: N_Area
        Pix_Area_m = Pix_Area{m};%找到对应的区域
        [Pix_Area_m_X, Pix_Area_m_Y] = ind2sub([image_size(1), image_size(2)], Pix_Area_m);%转化为坐标
        B_id = boundary(Pix_Area_m_X, Pix_Area_m_Y, 0.9);%提取边缘，找出边界索引
        BD_Area_m_X = Pix_Area_m_X(B_id);%边界的x坐标
        BD_Area_m_Y = Pix_Area_m_Y(B_id);%边界的y坐标
        %保存
        temp = temp + 1;%临时计数用
        PolySave(temp).AreaX = BD_Area_m_X;
        PolySave(temp).AreaY = BD_Area_m_Y;
        PolySave(temp).Color = camp(k, :);
        PolySave(temp).AreaSum = numel(Pix_Area_m);%计算面积
    end
end

%按照面积排序
N_Polygon = numel(PolySave);
Area_Max2Min = zeros(N_Polygon, 1);
Area_List = zeros(N_Polygon,1);
for k = 1: N_Polygon
    Area_List(k) = PolySave(k).AreaSum;%提取出面积
end
[~, Area_Max2Min] = sort(Area_List, 'descend');

%将每个多边形向外扩展1个像素
for k = 1: N_Polygon
    if ~isempty(PolySave(k).AreaX)
        if Area_List(k) < 9
            R = 0.5;
        elseif Area_List(k) < 16
            R = 1;
        else
            R = 1.5;
        end
        [xE, yE] = PolyExpand(PolySave(k).AreaX, PolySave(k).AreaY, R);
        xE(xE < 1) = 1; xE(xE > image_size(1)) = image_size(1);%防止超出画布边界
        yE(yE < 1) = 1; yE(yE > image_size(2)) = image_size(2);
        PolySave(k).AreaX = xE;%保存
        PolySave(k).AreaY = yE;
    end
end

%创建文件
f_id = fopen(filename_svg, 'w');
s=['<svg width="', num2str(image_size(1)), '" height="', num2str(image_size(1)), '">'];
fprintf(f_id, '%s \r\n', s);
%中间添加各个多边形
for k = 1: N_Polygon
    ID_k = Area_Max2Min(k);
    xSVG_k = PolySave(ID_k).AreaY;
    ySVG_k = PolySave(ID_k).AreaX;
    cSVG_k = PolySave(ID_k).Color;
    if numel(xSVG_k) > 3
        %坐标转换
        str_sum = '';
        for m = 1: numel(xSVG_k)
            str_1 = [num2str(xSVG_k(m)), ',', num2str(ySVG_k(m))];
            str_sum=[str_sum,str_1, ' '];
        end
        str_1 = [num2str(xSVG_k(1)), ',', num2str(ySVG_k(1))];
        str_sum = [str_sum,str_1];
        %颜色转换
        str_1 = ['rgb(',num2str(cSVG_k(1)*255), ',', num2str(cSVG_k(2)*255), ',', num2str(cSVG_k(3)*255), ')'];
        s=['<polygon fill="', str_1, '" stroke="', str_1, '" stroke-width="1" points="', str_sum, '"/>'];
        fprintf(f_id, '%s \r\n', s);
    end

end
%结束
s = ['</svg>'];
fprintf(f_id,'%s \r\n', s);
fclose(f_id);



function img = img_medfilt(I, windows)
%图片的中值滤波
    img_R = medfilt2(I(:, :, 1), [windows, windows]);
    img_G = medfilt2(I(:, :, 2), [windows, windows]);
    img_B = medfilt2(I(:, :, 3), [windows, windows]);
    img = uint8(zeros(size(I)));
    img(:, :, 1) = img_R; img(:, :, 2) = img_G; img(:, :, 3) = img_B;
end

function img = Del_IsolatedPixel(X)
%去除孤立的像素。如果该像素周围只有2个和它一样的像素的话，就认为它是孤立的
%耗时较长
%图像基本信息
hei = size(X, 1);
wid = size(X, 2);
num = hei * wid;%图像的像素总数
%X=double(X);不能double，浮点数从1索引，整数从0索引，不一样
X2 = NaN(hei + 2, wid + 2);%X2只用于检索
X2(2: end - 1, 2: end - 1) = X;
img = X;%X3只用于更改
for k = 1: num
    [k1, k2] = ind2sub([hei, wid], k);
    k1 = k1 + 1;k2 = k2 + 1;%由于X2在周围加了一圈，所以索引值也要加1
    %读取周边的9个数值，NaN为辅助值
    X_Id9 = X2(k1 - 1: k1 + 1, k2 - 1: k2 + 1);
    X_IdC = X_Id9(2, 2);%中央的点读取
    X_Id9(2, 2) = NaN;
    %删除所有的inf
    X_Id9_2 = X_Id9(:);
    [NaN_Id9, ~] = find(isnan(X_Id9_2));
    X_Id9_2(NaN_Id9, :) = [];
    %判断中间值和周围值是否只存在一个相等的
    if sum(X_Id9_2 == X_IdC) <= 2
        %如果是，把这个孤立像素替换为周围的某个点
        img(k) = mode(X_Id9_2);
    end
end
end

function [xE, yE] = PolyExpand(x, y, R)
%将每个多边形向外扩展1个像素
%将每个顶点，沿着相邻两个边 所合成的矢量方向，移动。如果落在图形内部，则反向
%多边形不能自相交
x=x(:);
y=y(:);
%判断是否收尾相交封闭
if x(1) == x(end) && y(1) == y(end)
    x2 = x;
    y2 = y;
else
    x2 = [x; x(1)];
    y2 = [y; y(1)];
end
%把点按照指定方向进行排列
Area_xy = trapz(x2, y2);
if Area_xy > 0
    x2 = flipud(x2);
    y2 = flipud(y2);
end
N = numel(x2);
%计算每个点与下一个点相连的向量
Dx = diff(x2);
Dy = diff(y2);
%开始按方向逐点移动
x3 = []; y3 = [];
for k = 1: N - 1
    %计算向量a和向量b
    if k == 1
        Da = [Dx(1), Dy(1)];
        Db = [Dx(N-1), Dy(N-1)];
    else
        Da = [Dx(k), Dy(k)];
        Db = [Dx(k - 1), Dy(k - 1)];
    end
    %归一化
    Da = Da / norm(Da);
    Db = Db / norm(Db);
    %如果向量a和向量b差积大于0，则证明是个凹陷点
    Dc = cross([Da, 0], [Db, 0]);
    Dab_Sin = Dc(3);
    %进行扩展判断
    if Dab_Sin > 0
        DR = Da - Db;%如果是凹点
        xy3 = [x2(k), y2(k)] + R * DR;
        %xy3=[x2(k),y2(k)]+R*DR/abs(Dab_Sin);保形膨胀，但是实际效果反而不好
        x3 = [x3; xy3(1)];
        y3 = [y3, xy3(2)];
    elseif Dab_Sin < 0
        DR = Db - Da;%如果是凸点
        xy3 = [x2(k), y2(k)] + R * DR;
        %xy3=[x2(k),y2(k)]+R*DR/abs(Dab_Sin);保形膨胀，但是实际效果反而不好
        x3 = [x3; xy3(1)];
        y3 = [y3, xy3(2)];
    elseif Dc(3) == 0
        %直接跳过
    end
end
xE = x3;
yE = y3;
end
end
