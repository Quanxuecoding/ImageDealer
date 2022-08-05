import cv2
from cv2 import dnn_superres
import os.path
import numpy as np
import time

# Create an SR object
sr = cv2.dnn_superres.DnnSuperResImpl_create()

# Read image


def cv_imread(file_path):
    cv_img = cv2.imdecode(np.fromfile(file_path, dtype=np.uint8), -1)
    # im decode读取的是rgb，如果后续需要opencv处理的话，需要转换成bgr，转换后图片颜色会变化
    # cv_img = cv2.cvtColor(cv_img, cv2.COLOR_RGB2BGR)
    return cv_img


img_path = input("请输入文件绝对路径：")
fileName = input("请输入文件名：")
# image = cv_imread(img_path+"\\"+fileName)
image = cv2.imread(img_path+"\\"+fileName)
extension = os.path.splitext(fileName)[1]
# print(extension)
fileName = os.path.splitext(fileName)[0]
# print(fileName)

way = input("启用更高质量输入y：(但是运行速度会明显下降)")
scale = int(input("请输入目标放大倍数：（支持2/3/4/8, 默认执行2, 更高质量不支持8）"))
if way == 'y':
    if scale not in (2, 3, 4):
        scale = 2
else:
    if scale not in (2, 3, 4, 8):
        scale = 2

# Read the desired model
if way == 'y':
    path = "./model/{}_x{}.pb".format("EDSR", scale)
    label = "edsr"
else:
    if scale == 8:
        path = "./model/{}_x{}.pb".format("LapSRN", scale)
        label = "lapsrn"
    else:
        path = "./model/{}_x{}.pb".format("ESPCN", scale)
        label = "espcn"
sr.readModel(path)

# Set the desired model and scale to get correct pre- and post-processing
sr.setModel(label, scale)

# Upscale the image
start = time.time()
result = sr.upsample(image)
# print("result okk")
print('用时  {:.3f} 秒'.format(time.time()-start))

# Save the image
cv2.imwrite(img_path+"\\"+fileName+"_rewrite_"+str(scale)+"x"+extension, result)
# cv2.imencode(extension, result)[1].tofile(img_path+"\\"+fileName+"_无损放大")
# print("save okk")
