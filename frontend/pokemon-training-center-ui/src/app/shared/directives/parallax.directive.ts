import { Directive, ElementRef, HostListener, Input, OnInit } from '@angular/core';

@Directive({
  selector: '[appParallax]',
  standalone: true
})
export class ParallaxDirective implements OnInit {
  @Input() parallaxSpeed = 0.3; // 0 = fixo, 1 = acompanha o scroll normal

  constructor(private el: ElementRef<HTMLElement>) {}

  ngOnInit(): void {
    this.updatePosition();
  }

  @HostListener('window:scroll')
  onScroll(): void {
    this.updatePosition();
  }

  private updatePosition(): void {
    const rect = this.el.nativeElement.getBoundingClientRect();
    const scrolled = window.scrollY;
    const elementTop = rect.top + scrolled;
    const offset = (scrolled - elementTop) * this.parallaxSpeed;

    this.el.nativeElement.style.transform = `translate3d(0, ${offset}px, 0)`;
  }
}