import type { Product } from '../types'

export const products: Product[] = [
  { id: 1, name: 'AirPods Max', category: 'Audio', price: 549, oldPrice: 599, rating: 4.8, reviewCount: 124, badge: 'Hot deal', colors: ['#e9e4db', '#65717d', '#1b2029'], image: 'https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb?auto=format&fit=crop&w=900&q=85' },
  { id: 2, name: 'iPhone 15 Pro', category: 'Phones', price: 999, rating: 4.9, reviewCount: 87, badge: 'New', colors: ['#cfc9bd', '#292929', '#51616a'], image: 'https://images.unsplash.com/photo-1696446701796-da61225697cc?auto=format&fit=crop&w=900&q=85' },
  { id: 3, name: 'MacBook Air M3', category: 'Computers', price: 1099, oldPrice: 1199, rating: 4.9, reviewCount: 214, colors: ['#d4d0c8', '#42434a'], image: 'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=900&q=85' },
  { id: 4, name: 'PlayStation 5 Slim', category: 'Gaming', price: 499, rating: 4.7, reviewCount: 42, badge: 'Best seller', image: 'https://images.unsplash.com/photo-1607853202273-797f1c22a38e?auto=format&fit=crop&w=900&q=85' },
  { id: 5, name: 'Apple Watch Ultra 2', category: 'Wearables', price: 799, rating: 4.8, reviewCount: 56, colors: ['#252525', '#f4a94d'], image: 'https://images.unsplash.com/photo-1434493789847-2f02dc6ca35d?auto=format&fit=crop&w=900&q=85' },
  { id: 6, name: 'Sony WH-1000XM5', category: 'Audio', price: 349, oldPrice: 399, rating: 4.8, reviewCount: 181, badge: 'Save 13%', colors: ['#272727', '#d7d1c5'], image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=85' },
  { id: 7, name: 'Magic Keyboard', category: 'Computers', price: 99, rating: 4.6, reviewCount: 38, image: 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=900&q=85' },
  { id: 8, name: 'Nintendo Switch OLED', category: 'Gaming', price: 349, rating: 4.7, reviewCount: 95, colors: ['#e74f57', '#fafafa'], image: 'https://images.unsplash.com/photo-1578303512597-81e6cc155b3e?auto=format&fit=crop&w=900&q=85' }
]
