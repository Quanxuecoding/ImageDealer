% ���ڶ�ȡͼ�񣬲�ת��Ϊ�Ҷ�ͼ��Ľű�
% ԭʼͼ�񱣴浽 [f,map]���Ҷ�ͼ�񱣴浽 f_gray
% ͼ����Ϣ���浽info
% �������нű�ǰ�����ȶ���ͼ���ļ�������·����Img_filename���򲻻��ѡ���ļ��ĶԻ���

if ~exist('Img_filename','var')
    % ��ȡͼ���ļ���������ȡͼ��[f,map]
    [filename, pathname] = uigetfile({'*.jpg;*.tif;*.png;*.gif;*.bmp','All Image Files';...
        '*.*','All Files' },'ѡ��һ��ͼ���ļ�');
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
%% �ж��Ƿ�ΪRGB��ɫͼ��
image_size = size(f);
if numel(image_size) == 3
    f_gray = rgb2gray(f(:,:,1:3));   % ����ɫͼ��ת��Ϊ�Ҷ�ͼ��
else
    f_gray = f;
end

%% �ж��Ƿ�Ϊ����ͼ��
if(~isempty(map))        % ���������ͼ��
    f_gray = ind2gray(f,map);
end

% imshow(f_gray);
