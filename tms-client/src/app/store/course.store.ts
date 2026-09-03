import { inject } from '@angular/core';

import {
  signalStore,
  withMethods,
  withState,
  patchState
} from '@ngrx/signals';

import {
  withEntities,
  setAllEntities,
  removeEntity
} from '@ngrx/signals/entities';

import { catchError, EMPTY } from 'rxjs';

import { CourseService } from '../services/course.service';
import { Course } from '../models/course.model';

type CourseState = {
  loading: boolean;
  error: string | null;
};

export const CourseStore = signalStore(
  { providedIn: 'root' },

  withState<CourseState>({
    loading: false,
    error: null
  }),

  withEntities<Course>(),

  withMethods(
    (
      store,
      courseService = inject(CourseService)
    ) => ({

      loadCourses() {
        patchState(store, {
          loading: true,
          error: null
        });

        courseService.getAll().subscribe({
          next: courses => {
            patchState(
              store,
              setAllEntities(courses),
              {
                loading: false
              }
            );
          },

          error: err => {
            console.error('Failed to load courses:', err);

            patchState(store, {
              loading: false,
              error: 'Failed to load courses.'
            });
          }
        });
      },

      loadMyCourses() {
        patchState(store, { loading: true, error: null });

        courseService.getMine().subscribe({
          next: courses => patchState(store, setAllEntities(courses), { loading: false }),
          error: () => patchState(store, {
            loading: false,
            error: 'Failed to load your assigned courses.'
          })
        });
      },

      deleteCourse(id: number) {

        // 1. Snapshot BEFORE deleting
        const previousSnapshot = store.entities();

        // 2. Remove immediately from UI
        patchState(
          store,
          removeEntity(id)
        );

        // 3. Tell backend
        courseService.delete(id)
          .pipe(
            catchError(err => {

              console.error(
                'Course deletion failed:',
                err
              );

              // 4. Roll back
              patchState(
                store,
                setAllEntities(previousSnapshot),
                {
                  error:
                    'Cannot delete course: active student enrollments exist.'
                }
              );

              return EMPTY;
            })
          )
          .subscribe();
      }
    })
  )
);
