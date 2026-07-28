import { Component, input, effect, computed } from "@angular/core";
import { RouterLink } from '@angular/router';
@Component({
selector: "app-course-detail",
imports: [RouterLink],
standalone: true,
templateUrl: "./course-detail.html",
styleUrl: './course-detail.scss'
})
export class CourseDetailComponent {

    id = input.required<string>();
    course = computed(() => {
      return {
        id: this.id(),
        title: `course-title (ID: ${this.id()})`,
        code: `CRS-${this.id()}`
      };
    });
    constructor() {effect(() => {
      console.log(`Loading course detail for ID: ${this.id()}`);
    });
  }
   
}

