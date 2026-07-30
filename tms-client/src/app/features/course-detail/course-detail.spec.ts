import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router'; // RouterLink እንዲሰራ ለመርዳት
import { CourseDetailComponent } from './course-detail'; // ስሙ ተስተካክሏል

describe('CourseDetailComponent', () => {
  let component: CourseDetailComponent;
  let fixture: ComponentFixture<CourseDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CourseDetailComponent], // ስሙ ተስተካክሏል
      providers: [provideRouter([])] // የ RouterLink ስህተትን ለመከላከል
    }).compileComponents();

    fixture = TestBed.createComponent(CourseDetailComponent);
    component = fixture.componentInstance;
    
    // ለ Signal input id መጀመሪያ እሴት (mock value) መስጠት
    fixture.componentRef.setInput('id', '123'); 
    
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
