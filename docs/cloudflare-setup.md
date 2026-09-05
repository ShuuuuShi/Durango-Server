# Cloudflare + Nginx Setup สำหรับ Durango Asset Bundles

## ปัญหาที่แก้
- VPS ไม่ไหวเมื่อคนโหลด bundle เยอะ → ย้าย static files ไป Cloudflare CDN
- ลด bandwidth บน VPS ได้ 90%+

## ภาพรวม

```
มือถือ玩家  →  Cloudflare CDN (cache bundles)  →  Nginx (VPS)  →  Durango Server
                ↑ 90%+ request จบตรงนี้            ↑ รับแค่ cache miss
```

---

## ขั้นตอนที่ 1: ตั้ง Nginx บน VPS

### 1.1 ติดตั้ง Nginx (ถ้ายังไม่มี)
```bash
ssh root@your-vps-ip

# Ubuntu/Debian
apt update && apt install -y nginx

# CentOS/RHEL
yum install -y nginx
```

### 1.2 อัพโหลด Bundle Files ไป VPS
```bash
# บนเครื่อง Windows (Git Bash)
# สร้างโฟลเดอร์บน VPS
ssh root@your-vps-ip "mkdir -p /opt/durango/AssetBundles-android"

# อัพโหลด bundle ทั้งหมด (910MB, ใช้เวลาตาม bandwidth)
scp -r "C:\Users\thana\Desktop\Durango Android\AssetBundles-android\*" \
  root@your-vps-ip:/opt/durango/AssetBundles-android/

# ถ้ามี bundle ชุด Windows/PC ด้วย
# scp -r /path/to/AssetBundles/* root@your-vps-ip:/opt/durango/AssetBundles/
```

### 1.3 ตั้ง Nginx Config
```bash
# คัดลอก config ที่เตรียมไว้
scp "C:\Users\thana\Desktop\Durango Opencode\docs\nginx-bundles.conf" \
  root@your-vps-ip:/etc/nginx/sites-available/durango

# บน VPS
ssh root@your-vps-ip

# เปิดใช้ config
ln -sf /etc/nginx/sites-available/durango /etc/nginx/sites-enabled/durango
rm -f /etc/nginx/sites-enabled/default

# ทดสอบ
nginx -t
# ต้องเห็น: "syntax is ok" + "test is successful"

# เริ่ม nginx
systemctl enable nginx
systemctl start nginx
```

### 1.4 ทดสอบว่า Nginx เสิร์ฟได้
```bash
# ทดสอบจากเครื่องอื่น
curl -I http://your-vps-ip/assetbundles/android/Info.android.5.2.1.json

# ต้องเห็น:
# HTTP/1.1 200 OK
# Cache-Control: public, max-age=31536000, immutable
# ...
```

### 1.5 เปิด Firewall Port 80
```bash
# Ubuntu UFW
ufw allow 80/tcp
ufw reload

# หรือ iptables
iptables -A INPUT -p tcp --dport 80 -j ACCEPT
```

---

## ขั้นตอนที่ 2: ตั้ง Cloudflare

### 2.1 สมัคร Cloudflare (ฟรี)
1. ไปที่ https://dash.cloudflare.com/sign-up
2. สมัครด้วย email + ตั้ง password
3. กด "Add a site"
4. กรอก domain ของคุณ (เช่น `durangoth.com`)
5. เลือก **Free** plan
6. Cloudflare จะสแกน DNS records ให้อัตโนมัติ

### 2.2 เปลี่ยน Nameserver ที่ Domain Registrar
Cloudflare จะให้ nameserver 2 ตัว เช่น:
```
ns1.cloudflare.com
ns2.cloudflare.com
```

ไปที่ domain registrar ของคุณ (Namecheap, GoDaddy, ฯลฯ):
1. เข้า Domain Management
2. เปลี่ยน Nameserver เป็นของ Cloudflare
3. รอ 5-30 นาที ให้ DNS propagate

### 2.3 ตั้ง DNS Record ใน Cloudflare
ใน Cloudflare Dashboard → DNS → Records:
```
Type    Name    Content           Proxy status
A       @       your-vps-ip       Proxied (橙色)
A       *       your-vps-ip       Proxied (橙色)
```

> **สำคัญ**: ต้องเปิด **Proxy** (สีส้ม) เท่านั้น ถ้าเป็น DNS only (สีเทา) จะไม่มี CDN

### 2.4 ตั้ง Cache Rules
ใน Cloudflare Dashboard → Rules → Cache Rules → Create rule:

**Rule 1: Cache Bundle Files (ตลอดกาล)**
```
Rule name:    Cache Asset Bundles
When:         http.request.uri.path contains "/assetbundles/"
Then:
  Cache eligibility:  Eligible for cache
  Cache TTL:          Override TTL → Respect Origin Headers
```

**Rule 2: Cache Index JSON (1 ชั่วโมง)**
```
Rule name:    Cache Bundle Index
When:         http.request.uri.path contains ".json"
Then:
  Cache eligibility:  Eligible for cache
  Cache TTL:          Override TTL → 1 hour
```

### 2.5 เปิด Compression
Cloudflare Dashboard → Speed → Optimization → Content Optimization:
- เปิด **Brotli** (ถ้ามี)
- เปิด **Early Hints**

### 2.6 ตั้ง Security (ป้องกัน abuse)
Cloudflare Dashboard → Security → WAF:
- เป็น Free plan พอแล้ว (มี DDoS protection อัตโนมัติ)

---

## ขั้นตอนที่ 3: แก้ Durango Server Config

### 3.1 แก้ config.json
แก้ไฟล์ `server/data/islands/isle01/config.json`:
```json
{
  "AssetBundleUrlBase": "http://durangoth.com",
  ...
}
```

> **สำคัญ**: 
> - ถ้า Cloudflare Proxy เปิดอยู่ → ใช้ `http://durangoth.com` (Cloudflare จัดการ SSL ให้)
> - ถ้าปิด Proxy → ต้องใช้ `https://` ถ้ามี SSL cert

### 3.2 Restart Server
```bash
# บน VPS
systemctl restart durango-server
# หรือ kill + reread ตามที่ใช้อยู่
```

---

## ขั้นตอนที่ 4: ทดสอบ

### 4.1 ทดสอบว่า Client ได้ URL ถูก
```bash
# ยิง /knock
curl "http://your-vps-ip:8190/knock?version=0.1.4&platform=Android&build=android-0.1.x"

# ดู assetbundle_url_root ต้องชี้ไป Cloudflare
# "assetbundle_url_root": "http://durangoth.com/assetbundles/android/"
```

### 4.2 ทดสอบว่า Cloudflare Cache ทำงาน
```bash
# ยิง bundle ผ่าน Cloudflare
curl -I "http://durangoth.com/assetbundles/android/integratedeffects.85f8f1bb62d9bad86ae61ca0a61be9bc.bundle"

# ครั้งแรก: ต้องเห็น "cf-cache-status: MISS" (ดึงจาก origin)
# ครั้งที่สอง: ต้องเห็น "cf-cache-status: HIT" (เสิร์ฟจาก cache)

# ดู header เพิ่มเติม:
# cf-cache-status: HIT
# cache-control: public, max-age=31536000, immutable
# cf-ray: xxx  ← Cloudflare edge ที่เสิร์ฟ
```

---

## สรุป Architecture หลังตั้งค่า

```
มือถือ玩家  →  Cloudflare CDN (edge ทั่วโลก)
                    │
                    ├─ HIT  → เสิร์ฟจาก cache (เร็วสุด, ไม่โหลด VPS)
                    │
                    └─ MISS → ดึงจาก Nginx (VPS) → Cache ไว้ที่ edge
                                  │
                                  └─ DurangoServer process (ไม่ต้อง serve bundles อีก)
```

### ผลลัพธ์ที่คาด
| ก่อน | หลัง |
|------|------|
| VPS serve 910MB × ทุกคน | VPS serve เฉพาะ cache miss (~10%) |
| Bandwidth: ~910GB/1000 users | Bandwidth: ~91GB/1000 users |
| CPU: serve static files | CPU: เหลือเล่นเกมอย่างเดียว |
| DDoS: ตายทันที | DDoS: Cloudflare ดูดซับให้ |

---

## หมายเหตุ

### ถ้าไม่มี Domain (ใช้ IP ตรง)
Cloudflare ต้องมี domain — ถ้าไม่มี domain ใช้วิธี:
1. ซื้อ domain ถูกๆ (.xyz ราคา ~$1/ปี) ที่ Namecheap
2. หรือใช้ Cloudflare Tunnel (ตั้งยากกว่า)
3. หรือใช้ Bunny CDN (ไม่ต้องมี domain, $0.01/GB)

### ถ้า bundle มีการอัปเดต
1. อัพโหลด bundle ใหม่ไปที่ VPS (ชื่อเปลี่ยน = cache busting อัตโนมัติ)
2. อัพโหลด Info.android.5.2.1.json ใหม่ (index มี TTL 1 ชม. = อัปเดตภายใน 1 ชม.)
3. ไม่ต้องแก้ Cloudflare config อะไรเพิ่ม
