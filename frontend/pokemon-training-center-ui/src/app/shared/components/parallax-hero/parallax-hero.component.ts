import { Component, Input } from '@angular/core';
import { ParallaxDirective } from '../../directives/parallax.directive';

@Component({
  selector: 'app-parallax-hero',
  standalone: true,
  imports: [ParallaxDirective],
  template: `
    <section class="hero">
      <div class="hero__layer hero__layer--back" appParallax [parallaxSpeed]="0.15"></div>
      <div class="hero__layer hero__layer--mid" appParallax [parallaxSpeed]="0.3"></div>
      <div class="hero__content">
        <p class="hero__eyebrow">{{ eyebrow }}</p>
        <h1 class="hero__title">{{ title }}</h1>
        <p class="hero__subtitle">{{ subtitle }}</p>
      </div>
      <div class="hero__scroll-hint">
        <span></span>
      </div>
    </section>
  `,
  styleUrl: './parallax-hero.component.scss'
})
export class ParallaxHeroComponent {
  @Input() eyebrow = '';
  @Input() title = '';
  @Input() subtitle = '';
}