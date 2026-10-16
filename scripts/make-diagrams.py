"""Regenerate the editable draw.io diagrams. Optional documentation helper, not used by the website."""
from pathlib import Path
import xml.etree.ElementTree as E

doc = E.Element('mxfile', host='app.diagrams.net', type='device')
def page(name, width=1400, height=1000):
    d=E.SubElement(doc,'diagram',name=name,id=name.replace(' ','-'))
    m=E.SubElement(d,'mxGraphModel',dx=str(width),dy=str(height),grid='1',gridSize='10',page='1',pageScale='1',pageWidth=str(width),pageHeight=str(height))
    root=E.SubElement(m,'root');E.SubElement(root,'mxCell',id='0');E.SubElement(root,'mxCell',id='1',parent='0');return root
def box(root,id,text,x,y,w,h,style=''):
    cell=E.SubElement(root,'mxCell',id=id,value=text,style='whiteSpace=wrap;html=1;fontFamily=Arial;fontSize=13;fillColor=#F8F6F1;strokeColor=#B89156;fontColor=#172C49;'+style,vertex='1',parent='1')
    E.SubElement(cell,'mxGeometry',x=str(x),y=str(y),width=str(w),height=str(h),**{'as':'geometry'});return id
def edge(root,id,a,b,label=''):
    c=E.SubElement(root,'mxCell',id=id,value=label,style='edgeStyle=orthogonalEdgeStyle;html=1;endArrow=block;strokeColor=#67798F;fontSize=11;',edge='1',parent='1',source=a,target=b)
    E.SubElement(c,'mxGeometry',relative='1',**{'as':'geometry'})

r=page('Use Case')
box(r,'title','AURORA HOTEL — USE CASE',300,20,750,50,'fontSize=23;fontStyle=1;fillColor=#172C49;fontColor=#FFFFFF;')
for id,label,y in [('visitor','Khách chưa đăng nhập',150),('customer','Customer',350),('staff','Receptionist',570),('admin','Admin',800)]:box(r,id,label,30,y,140,90,'shape=umlActor;')
items=[('search','Xem / tìm / chi tiết phòng',130),('account','Đăng ký / đăng nhập / hồ sơ',240),('booking','Đặt / xem / hủy đơn của mình',350),('reserve','Tạo / sửa / xác nhận / từ chối đơn',460),('stay','Check-in / dịch vụ / folio / thu tiền / checkout',570),('board','Tình trạng phòng / Clean-Dirty',680),('catalog','CRUD phòng / loại / dịch vụ / OutOfOrder',790),('manage','Tài khoản / phân quyền / báo cáo',900)]
for id,label,y in items:box(r,id,label,390,y,510,70,'ellipse;')
for i,(a,b) in enumerate([('visitor','search'),('visitor','account'),('customer','search'),('customer','account'),('customer','booking'),('staff','reserve'),('staff','stay'),('staff','board'),('admin','reserve'),('admin','stay'),('admin','board'),('admin','catalog'),('admin','manage')]):edge(r,'e'+str(i),a,b)

r=page('ERD',1500,1100)
box(r,'title','AURORA HOTEL — SCHEMA CHUNG',390,20,680,50,'fontSize=22;fontStyle=1;fillColor=#172C49;fontColor=#FFFFFF;')
entities=[('users','AspNetUsers / Identity','Id PK<br>Email / UserName<br>PasswordHash / SecurityStamp',50,140),('roles','AspNetRoles + UserRoles','RoleId / UserId<br>Admin | Receptionist | Customer',50,410),('guest','GuestProfiles','Id PK · UserId FK nullable UNIQUE<br>FullName / Email / Phone / Notes',410,140),('type','RoomTypes','Id PK · Name · Capacity<br>Description',1090,140),('room','Rooms','Id PK · RoomNumber UNIQUE<br>RoomTypeId FK · NightlyRate<br>ImagePath / Description / Amenities<br>Clean-Dirty · Active-OutOfOrder',1090,390),('reservation','Reservations','Id PK · ConfirmationCode UNIQUE<br>GuestProfileId FK · RoomId FK<br>ArrivalDate / DepartureDate / GuestCount<br>NightlyRateSnapshot / Status / Source<br>CreatedByUserId FK / timestamps',410,430),('folio','Folios','Id PK · ReservationId UNIQUE FK<br>Status / CreatedAt / ClosedAt',410,790),('item','FolioItems','Id PK · FolioId FK · ServiceId FK?<br>Kind / DescriptionSnapshot<br>Quantity / UnitPriceSnapshot',800,790),('service','Services','Id PK · Name · Unit<br>UnitPrice · IsActive',1130,790),('payment','Payments','Id PK · FolioId UNIQUE FK<br>Amount / Method / PaidAt<br>ReceivedByUserId FK',50,790)]
for id,name,fields,x,y in entities:box(r,id,'<b>'+name+'</b><hr>'+fields,x,y,285,160,'align=left;verticalAlign=top;spacing=12;')
for i,(a,b,label) in enumerate([('users','guest','1 — 0..1'),('users','roles','N — N'),('guest','reservation','1 — N'),('type','room','1 — N'),('room','reservation','1 — N'),('reservation','folio','1 — 0..1'),('folio','item','1 — N'),('service','item','1 — N'),('folio','payment','1 — 0..1'),('users','payment','nhân viên thu')]):edge(r,'rel'+str(i),a,b,label)

r=page('Wireframe Customer',1360,1200)
box(r,'header','LOGO AURORA        Trang chủ       Phòng & kỳ nghỉ       Đơn của tôi       Tài khoản       [Đặt phòng]',30,20,1300,75,'fillColor=#FFFFFF;')
box(r,'hero','ẢNH KHÁCH SẠN<br><br><b>Chạm vào những bình yên.</b><br>Giới thiệu ngắn / Khám phá',30,95,1300,350,'fillColor=#172C49;fontColor=#FFFFFF;fontSize=28;align=left;spacing=40;')
box(r,'search','Kỳ nghỉ của bạn      [Ngày nhận]      [Ngày trả]      [Số khách]      [Tìm phòng trống]',100,400,1160,110,'fillColor=#FFFFFF;')
box(r,'intro','GIỚI THIỆU                                  NỘI DUNG / LIÊN KẾT TÌM PHÒNG',100,550,1160,150,'fontSize=20;')
for i in range(3):box(r,'card'+str(i),'Ảnh phòng<br><br>Loại phòng / sức chứa<br>Tiện nghi / giá mỗi đêm / Chi tiết →',100+400*i,750,360,235)
box(r,'footer','TIỆN NGHI / DỊCH VỤ / LIÊN HỆ / FOOTER',100,1030,1160,100)
r=page('Wireframe Staff',1360,1050)
box(r,'side','AURORA<br><br>Tổng quan<br>Lễ tân<br>Đặt phòng<br>Tình trạng phòng<br>Khách hàng<br><br>QUẢN TRỊ<br>Phòng / Loại phòng<br>Dịch vụ<br>Tài khoản & quyền',20,20,220,980,'fillColor=#172C49;fontColor=#FFFFFF;align=left;verticalAlign=top;spacing=25;')
box(r,'top','Tiêu đề khu vực                                                 Ngày làm việc / Tài khoản / Đăng xuất',260,20,1080,70)
box(r,'heading','TỔNG QUAN                                             [Tạo đặt phòng]',280,120,1020,60,'fontSize=23;')
box(r,'filter','KHOẢNG BÁO CÁO                                      [Từ ngày]  [Đến ngày]  [Áp dụng]',280,210,1020,85)
for i,label in enumerate(['Tiền đã thu','Đơn đặt','Phòng có khách','Cần dọn']):box(r,'stat'+str(i),label+'<br><br><b>SỐ LIỆU DB</b>',280+260*i,335,235,135)
box(r,'chart','PHÂN BỐ TRẠNG THÁI ĐƠN',280,505,610,220)
box(r,'quick','THAO TÁC NHANH<br><br>Lễ tân / Room Board',915,505,385,220)
box(r,'table','ĐẶT PHÒNG GẦN ĐÂY<br><br>Mã đơn | Khách | Phòng | Kỳ ở | Tiền | Trạng thái | Chi tiết<br><br>Bảng cuộn ngang khi màn hình nhỏ',280,760,1020,190)

out=Path(__file__).resolve().parents[1]/'docs'/'diagrams.drawio'
E.indent(doc); E.ElementTree(doc).write(out,encoding='utf-8',xml_declaration=True)
print(out)
